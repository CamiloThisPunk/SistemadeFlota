using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace TrackWay.API.Middleware;

/// <summary>
/// Middleware para manejo global de excepciones con ProblemDetails
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log sin datos sensibles (GDPR compliant)
        _logger.LogError(exception, "Error no controlado en {Path}", context.Request.Path);

        var (statusCode, title, detail) = exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, "Solicitud inválida", exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Recurso no encontrado", exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "No autorizado", "Acceso denegado"),
            InvalidOperationException => (HttpStatusCode.Conflict, "Operación inválida", exception.Message),
            FluentValidation.ValidationException validationEx => 
                (HttpStatusCode.BadRequest, "Error de validación", string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))),
            _ => (HttpStatusCode.InternalServerError, "Error del servidor", 
                _env.IsDevelopment() ? exception.Message : "Ha ocurrido un error interno")
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{(int)statusCode}"
        };

        // Agregar trace ID para debugging
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        // En desarrollo, incluir stack trace
        if (_env.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }
}

/// <summary>
/// Middleware para Rate Limiting simple (en memoria)
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly Dictionary<string, RateLimitInfo> _clients = new();
    private static readonly object _lock = new();
    
    private const int MaxRequestsPerMinute = 100;
    private const int MaxRequestsPerSecond = 10;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        
        if (IsRateLimited(clientId))
        {
            _logger.LogWarning("Rate limit excedido para cliente {ClientId}", clientId);
            
            context.Response.StatusCode = 429;
            context.Response.ContentType = "application/problem+json";
            
            var problem = new ProblemDetails
            {
                Status = 429,
                Title = "Demasiadas solicitudes",
                Detail = "Has excedido el límite de solicitudes. Intenta de nuevo en un momento.",
                Type = "https://httpstatuses.com/429"
            };
            
            context.Response.Headers.Append("Retry-After", "60");
            await context.Response.WriteAsJsonAsync(problem);
            return;
        }

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Usar IP + User-Agent como identificador (sin datos personales)
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userId = context.User?.Identity?.Name ?? "anonymous";
        return $"{ip}:{userId}";
    }

    private static bool IsRateLimited(string clientId)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            
            if (!_clients.TryGetValue(clientId, out var info))
            {
                _clients[clientId] = new RateLimitInfo { RequestCount = 1, WindowStart = now };
                return false;
            }

            // Limpiar ventana si pasó más de un minuto
            if ((now - info.WindowStart).TotalMinutes >= 1)
            {
                info.RequestCount = 1;
                info.WindowStart = now;
                return false;
            }

            info.RequestCount++;
            
            // Límite por minuto
            if (info.RequestCount > MaxRequestsPerMinute)
                return true;

            // Límite por segundo (burst)
            var secondsElapsed = (now - info.WindowStart).TotalSeconds;
            if (secondsElapsed > 0 && info.RequestCount / secondsElapsed > MaxRequestsPerSecond)
                return true;

            return false;
        }
    }

    private class RateLimitInfo
    {
        public int RequestCount { get; set; }
        public DateTime WindowStart { get; set; }
    }
}

/// <summary>
/// Extensions para registrar middlewares
/// </summary>
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }

    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RateLimitingMiddleware>();
    }
}
