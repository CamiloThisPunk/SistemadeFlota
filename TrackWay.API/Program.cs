using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using TrackWay.Domain.Interfaces;
using TrackWay.Infrastructure.Persistence;
using TrackWay.Infrastructure.Repositories;
using TrackWay.Infrastructure.Services;
using TrackWay.Application.Auth.Handlers;

var builder = WebApplication.CreateBuilder(args);

// ============ Servicios Core ============
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ============ Entity Framework Core ============
builder.Services.AddDbContext<TrackWayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar IAuthDbContext
builder.Services.AddScoped<IAuthDbContext>(sp => sp.GetRequiredService<TrackWayDbContext>());

// ============ Repository Pattern + UoW ============
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ============ MediatR (CQRS) ============
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(TrackWay.Application.Fleet.Commands.CreateVehicleCommand).Assembly);
});

// ============ FluentValidation ============
builder.Services.AddValidatorsFromAssemblyContaining<TrackWay.Application.Fleet.Validators.CreateVehicleValidator>();

// ============ Auth Services ============
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// ============ JWT Authentication ============
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "DefaultDevelopmentSecretKey12345678901234567890!";
var key = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("GerentePlus", policy => policy.RequireRole("Admin", "Gerente"));
    options.AddPolicy("AllRoles", policy => policy.RequireRole("Admin", "Gerente", "Asistente"));
});

// ============ CORS para React ============
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:3002", "http://localhost:3003", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============ Swagger/OpenAPI ============
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ============ Database Migration & Seeding ============
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TrackWayDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        
        // Aplicar migraciones pendientes
        await context.Database.MigrateAsync();
        
        // Ejecutar seeder
        await DbSeeder.SeedAsync(context, passwordHasher);
        
        Console.WriteLine("✅ Base de datos inicializada correctamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error al inicializar la base de datos: {ex.Message}");
    }
}

// ============ Middleware Pipeline ============
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TrackWay API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowFrontend");

// JWT Auth middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
