using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using TrackWay.Application.SuperAdmin;
using System.Text;

namespace TrackWay.API.Controllers.SuperAdmin;

/// <summary>
/// Controller para operaciones de SuperAdmin (gestión global SaaS)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")] // Permitir Admin temporalmente para pruebas
public class SuperAdminController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public SuperAdminController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /// <summary>
    /// Obtiene KPIs del dashboard SuperAdmin
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<SuperAdminDashboardDto>> GetDashboard()
    {
        var result = await _mediator.Send(new GetSuperAdminDashboardQuery());
        return Ok(result);
    }
    
    /// <summary>
    /// Obtiene lista paginada de tenants con filtros
    /// </summary>
    [HttpGet("tenants")]
    public async Task<ActionResult<PagedTenantsResult>> GetTenants(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? planId = null,
        [FromQuery] bool? activo = null)
    {
        var query = new GetAllTenantsQuery(pageNumber, pageSize, search, planId, activo);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    /// <summary>
    /// Obtiene estadísticas por plan de suscripción
    /// </summary>
    [HttpGet("plans/stats")]
    public async Task<ActionResult<List<PlanStatsDto>>> GetPlanStats()
    {
        var result = await _mediator.Send(new GetPlanStatsQuery());
        return Ok(result);
    }
    
    /// <summary>
    /// Obtiene lista de planes disponibles
    /// </summary>
    [HttpGet("plans")]
    public async Task<ActionResult<List<SubscriptionPlanDto>>> GetPlans()
    {
        var result = await _mediator.Send(new GetSubscriptionPlansQuery());
        return Ok(result);
    }
    
    /// <summary>
    /// Crea un nuevo tenant
    /// </summary>
    [HttpPost("tenants")]
    public async Task<ActionResult<CreateTenantResult>> CreateTenant([FromBody] CreateTenantCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.Success)
            return BadRequest(new { error = result.Error });
        
        return CreatedAtAction(nameof(GetTenants), new { id = result.TenantId }, result);
    }
    
    /// <summary>
    /// Actualiza un tenant existente
    /// </summary>
    [HttpPut("tenants/{id}")]
    public async Task<ActionResult<UpdateTenantResult>> UpdateTenant(int id, [FromBody] UpdateTenantRequest request)
    {
        var command = new UpdateTenantCommand(
            id, 
            request.Nombre, 
            request.EmailContacto, 
            request.Telefono, 
            request.Direccion, 
            request.NuevoPlanId
        );
        
        var result = await _mediator.Send(command);
        
        if (!result.Success)
            return BadRequest(new { error = result.Error });
        
        return Ok(result);
    }
    
    /// <summary>
    /// Activa/Desactiva un tenant
    /// </summary>
    [HttpPost("tenants/{id}/toggle")]
    public async Task<ActionResult<ToggleTenantResult>> ToggleTenantStatus(int id)
    {
        var result = await _mediator.Send(new ToggleTenantStatusCommand(id));
        
        if (!result.Success)
            return BadRequest(new { error = result.Error });
        
        return Ok(result);
    }
    
    /// <summary>
    /// Exporta la lista de tenants a CSV
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportTenantsCsv()
    {
        var result = await _mediator.Send(new GetAllTenantsQuery(1, 1000)); // Obtener todos
        
        var csv = new StringBuilder();
        csv.AppendLine("ID,Nombre,RUC,Email,Telefono,Plan,Precio,Estado,Usuarios,Vehiculos,Mantenimientos,Fecha Creacion");
        
        foreach (var tenant in result.Items)
        {
            csv.AppendLine($"{tenant.Id}," +
                $"\"{tenant.Nombre}\"," +
                $"{tenant.RUC}," +
                $"{tenant.EmailContacto}," +
                $"{tenant.Telefono ?? ""}," +
                $"{tenant.Plan?.Nombre ?? "Sin Plan"}," +
                $"${tenant.Plan?.PrecioMensual ?? 0}," +
                $"{(tenant.Activo ? "Activa" : "Inactiva")}," +
                $"{tenant.TotalUsuarios}," +
                $"{tenant.TotalVehiculos}," +
                $"{tenant.TotalMantenimientos}," +
                $"{tenant.FechaCreacion:yyyy-MM-dd}");
        }
        
        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"tenants_export_{DateTime.Now:yyyyMMdd}.csv");
    }
}

/// <summary>
/// Request para actualizar un tenant
/// </summary>
public record UpdateTenantRequest(
    string Nombre,
    string EmailContacto,
    string? Telefono,
    string? Direccion,
    int? NuevoPlanId
);
