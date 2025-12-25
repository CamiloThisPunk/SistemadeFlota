using MediatR;
using Microsoft.EntityFrameworkCore;
using TrackWay.Domain.Entities.SaaS;

namespace TrackWay.Application.SuperAdmin;

#region DbContext Interface

/// <summary>
/// Interface for SaaS database operations
/// </summary>
public interface ISaaSDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<TenantSubscription> TenantSubscriptions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

#endregion

#region DTOs

/// <summary>
/// DTO para KPIs del dashboard SuperAdmin
/// </summary>
public record SuperAdminDashboardDto(
    int TotalEmpresas,
    int EmpresasActivas,
    int EmpresasInactivas,
    int TotalUsuarios,
    double PromedioUsuariosPorEmpresa,
    int TotalVehiculos,
    int TotalMantenimientos
);

/// <summary>
/// DTO para información de un Tenant
/// </summary>
public record TenantDto(
    int Id,
    string Nombre,
    string RUC,
    string EmailContacto,
    string? Telefono,
    bool Activo,
    DateTime FechaCreacion,
    int TotalUsuarios,
    int TotalVehiculos,
    int TotalMantenimientos,
    SubscriptionPlanDto? Plan
);

/// <summary>
/// DTO para plan de suscripción
/// </summary>
public record SubscriptionPlanDto(
    int Id,
    string Nombre,
    string Descripcion,
    decimal PrecioMensual,
    int MaxUsuarios,
    int MaxVehiculos,
    string Tier,
    string ColorHex
);

/// <summary>
/// DTO para estadísticas de un plan
/// </summary>
public record PlanStatsDto(
    int PlanId,
    string Nombre,
    decimal PrecioMensual,
    string ColorHex,
    int TotalEmpresas,
    double PorcentajeDelTotal,
    int MaxVehiculos,
    double PromedioUsoVehiculos,
    int EmpresasCercaDelLimite
);

/// <summary>
/// Resultado paginado para la tabla de tenants
/// </summary>
public record PagedTenantsResult(
    List<TenantDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

#endregion

#region Queries

/// <summary>
/// Query para obtener KPIs del dashboard SuperAdmin
/// </summary>
public record GetSuperAdminDashboardQuery() : IRequest<SuperAdminDashboardDto>;

/// <summary>
/// Query para obtener lista paginada de tenants
/// </summary>
public record GetAllTenantsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    int? PlanId = null,
    bool? Activo = null
) : IRequest<PagedTenantsResult>;

/// <summary>
/// Query para obtener estadísticas por plan
/// </summary>
public record GetPlanStatsQuery() : IRequest<List<PlanStatsDto>>;

/// <summary>
/// Query para obtener todos los planes disponibles
/// </summary>
public record GetSubscriptionPlansQuery() : IRequest<List<SubscriptionPlanDto>>;

#endregion

#region Commands

/// <summary>
/// Comando para crear un nuevo tenant
/// </summary>
public record CreateTenantCommand(
    string Nombre,
    string RUC,
    string EmailContacto,
    string? Telefono,
    string? Direccion,
    int PlanId
) : IRequest<CreateTenantResult>;

public record CreateTenantResult(bool Success, int? TenantId = null, string? Error = null);

/// <summary>
/// Comando para actualizar un tenant
/// </summary>
public record UpdateTenantCommand(
    int TenantId,
    string Nombre,
    string EmailContacto,
    string? Telefono,
    string? Direccion,
    int? NuevoPlanId = null
) : IRequest<UpdateTenantResult>;

public record UpdateTenantResult(bool Success, string? Error = null);

/// <summary>
/// Comando para activar/desactivar un tenant
/// </summary>
public record ToggleTenantStatusCommand(int TenantId) : IRequest<ToggleTenantResult>;

public record ToggleTenantResult(bool Success, bool NuevoEstado, string? Error = null);

#endregion

#region Handlers

public class GetSuperAdminDashboardHandler : IRequestHandler<GetSuperAdminDashboardQuery, SuperAdminDashboardDto>
{
    private readonly ISaaSDbContext _context;
    
    public GetSuperAdminDashboardHandler(ISaaSDbContext context)
    {
        _context = context;
    }
    
    public async Task<SuperAdminDashboardDto> Handle(GetSuperAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _context.Tenants.ToListAsync(cancellationToken);
        
        var totalEmpresas = tenants.Count;
        var activas = tenants.Count(t => t.Activo);
        var inactivas = totalEmpresas - activas;
        
        var totalUsuarios = tenants.Sum(t => t.TotalUsuarios);
        var totalVehiculos = tenants.Sum(t => t.TotalVehiculos);
        var totalMantenimientos = tenants.Sum(t => t.TotalMantenimientos);
        
        var promedioUsuarios = totalEmpresas > 0 ? (double)totalUsuarios / totalEmpresas : 0;
        
        return new SuperAdminDashboardDto(
            totalEmpresas,
            activas,
            inactivas,
            totalUsuarios,
            Math.Round(promedioUsuarios, 1),
            totalVehiculos,
            totalMantenimientos
        );
    }
}

public class GetAllTenantsHandler : IRequestHandler<GetAllTenantsQuery, PagedTenantsResult>
{
    private readonly ISaaSDbContext _context;
    
    public GetAllTenantsHandler(ISaaSDbContext context)
    {
        _context = context;
    }
    
    public async Task<PagedTenantsResult> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tenants
            .Include(t => t.Subscriptions)
            .ThenInclude(s => s.Plan)
            .AsQueryable();
        
        // Filtro de búsqueda
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(t => 
                t.Nombre.ToLower().Contains(search) || 
                t.RUC.Contains(search) ||
                t.EmailContacto.ToLower().Contains(search));
        }
        
        // Filtro por plan
        if (request.PlanId.HasValue)
        {
            query = query.Where(t => t.Subscriptions.Any(s => s.SubscriptionPlanId == request.PlanId && s.FechaFin == null));
        }
        
        // Filtro por estado
        if (request.Activo.HasValue)
        {
            query = query.Where(t => t.Activo == request.Activo.Value);
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        
        var tenants = await query
            .OrderByDescending(t => t.FechaCreacion)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        var items = tenants.Select(t => 
        {
            var activeSub = t.Subscriptions.FirstOrDefault(s => s.FechaFin == null);
            var plan = activeSub?.Plan;
            
            return new TenantDto(
                t.Id,
                t.Nombre,
                t.RUC,
                t.EmailContacto,
                t.Telefono,
                t.Activo,
                t.FechaCreacion,
                t.TotalUsuarios,
                t.TotalVehiculos,
                t.TotalMantenimientos,
                plan != null ? new SubscriptionPlanDto(
                    plan.Id,
                    plan.Nombre,
                    plan.Descripcion,
                    plan.PrecioMensual,
                    plan.MaxUsuarios,
                    plan.MaxVehiculos,
                    plan.Tier.ToString(),
                    plan.ColorHex
                ) : null
            );
        }).ToList();
        
        return new PagedTenantsResult(items, totalCount, request.PageNumber, request.PageSize, totalPages);
    }
}

public class GetPlanStatsHandler : IRequestHandler<GetPlanStatsQuery, List<PlanStatsDto>>
{
    private readonly ISaaSDbContext _context;
    
    public GetPlanStatsHandler(ISaaSDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<PlanStatsDto>> Handle(GetPlanStatsQuery request, CancellationToken cancellationToken)
    {
        var planes = await _context.SubscriptionPlans
            .Include(p => p.Subscriptions)
            .ThenInclude(s => s.Tenant)
            .Where(p => p.Activo)
            .ToListAsync(cancellationToken);
        
        var totalEmpresas = await _context.Tenants.CountAsync(cancellationToken);
        
        var stats = planes.Select(plan =>
        {
            var suscripcionesActivas = plan.Subscriptions.Where(s => s.FechaFin == null).ToList();
            var tenantsEnPlan = suscripcionesActivas.Select(s => s.Tenant).Where(t => t != null).ToList();
            var cantidadEmpresas = tenantsEnPlan.Count;
            var porcentaje = totalEmpresas > 0 ? (cantidadEmpresas * 100.0 / totalEmpresas) : 0;
            
            var promedioVehiculos = tenantsEnPlan.Count > 0 
                ? tenantsEnPlan.Average(t => t.TotalVehiculos) 
                : 0;
            
            var usoPromedio = plan.MaxVehiculos > 0 
                ? (promedioVehiculos * 100.0 / plan.MaxVehiculos) 
                : 0;
            
            var cercaDelLimite = tenantsEnPlan.Count(t => 
                (t.TotalVehiculos * 100.0 / plan.MaxVehiculos) >= 80);
            
            return new PlanStatsDto(
                plan.Id,
                plan.Nombre,
                plan.PrecioMensual,
                plan.ColorHex,
                cantidadEmpresas,
                Math.Round(porcentaje, 1),
                plan.MaxVehiculos,
                Math.Round(usoPromedio, 1),
                cercaDelLimite
            );
        }).ToList();
        
        return stats;
    }
}

public class GetSubscriptionPlansHandler : IRequestHandler<GetSubscriptionPlansQuery, List<SubscriptionPlanDto>>
{
    private readonly ISaaSDbContext _context;
    
    public GetSubscriptionPlansHandler(ISaaSDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<SubscriptionPlanDto>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _context.SubscriptionPlans
            .Where(p => p.Activo)
            .OrderBy(p => p.PrecioMensual)
            .ToListAsync(cancellationToken);
        
        return plans.Select(p => new SubscriptionPlanDto(
            p.Id,
            p.Nombre,
            p.Descripcion,
            p.PrecioMensual,
            p.MaxUsuarios,
            p.MaxVehiculos,
            p.Tier.ToString(),
            p.ColorHex
        )).ToList();
    }
}

public class CreateTenantHandler : IRequestHandler<CreateTenantCommand, CreateTenantResult>
{
    private readonly ISaaSDbContext _context;
    
    public CreateTenantHandler(ISaaSDbContext context)
    {
        _context = context;
    }
    
    public async Task<CreateTenantResult> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar RUC único
            var existeRuc = await _context.Tenants.AnyAsync(t => t.RUC == request.RUC, cancellationToken);
            if (existeRuc)
                return new CreateTenantResult(false, Error: "Ya existe una empresa con ese RUC");
            
            // Verificar que el plan existe
            var plan = await _context.SubscriptionPlans.FindAsync(new object[] { request.PlanId }, cancellationToken);
            if (plan == null)
                return new CreateTenantResult(false, Error: "El plan seleccionado no existe");
            
            // Crear tenant
            var tenant = Tenant.Create(
                request.Nombre,
                request.RUC,
                request.EmailContacto,
                request.Telefono,
                request.Direccion
            );
            
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync(cancellationToken);
            
            // Crear suscripción
            var subscription = TenantSubscription.Create(tenant.Id, request.PlanId);
            _context.TenantSubscriptions.Add(subscription);
            await _context.SaveChangesAsync(cancellationToken);
            
            return new CreateTenantResult(true, tenant.Id);
        }
        catch (Exception ex)
        {
            return new CreateTenantResult(false, Error: ex.Message);
        }
    }
}

public class UpdateTenantHandler : IRequestHandler<UpdateTenantCommand, UpdateTenantResult>
{
    private readonly ISaaSDbContext _context;
    
    public UpdateTenantHandler(ISaaSDbContext context)
    {
        _context = context;
    }
    
    public async Task<UpdateTenantResult> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _context.Tenants
                .Include(t => t.Subscriptions)
                .FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken);
            
            if (tenant == null)
                return new UpdateTenantResult(false, "Empresa no encontrada");
            
            // Actualizar datos básicos
            tenant.ActualizarDatos(
                request.Nombre,
                request.EmailContacto,
                request.Telefono,
                request.Direccion
            );
            
            // Si se cambia de plan
            if (request.NuevoPlanId.HasValue)
            {
                var nuevoPlan = await _context.SubscriptionPlans.FindAsync(
                    new object[] { request.NuevoPlanId.Value }, cancellationToken);
                
                if (nuevoPlan == null)
                    return new UpdateTenantResult(false, "El plan seleccionado no existe");
                
                // Finalizar suscripción actual
                var suscripcionActual = tenant.Subscriptions.FirstOrDefault(s => s.FechaFin == null);
                if (suscripcionActual != null)
                {
                    suscripcionActual.Finalizar();
                }
                
                // Crear nueva suscripción
                var nuevaSuscripcion = TenantSubscription.Create(tenant.Id, request.NuevoPlanId.Value);
                _context.TenantSubscriptions.Add(nuevaSuscripcion);
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            return new UpdateTenantResult(true);
        }
        catch (Exception ex)
        {
            return new UpdateTenantResult(false, ex.Message);
        }
    }
}

public class ToggleTenantStatusHandler : IRequestHandler<ToggleTenantStatusCommand, ToggleTenantResult>
{
    private readonly ISaaSDbContext _context;
    
    public ToggleTenantStatusHandler(ISaaSDbContext context)
    {
        _context = context;
    }
    
    public async Task<ToggleTenantResult> Handle(ToggleTenantStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(new object[] { request.TenantId }, cancellationToken);
            
            if (tenant == null)
                return new ToggleTenantResult(false, false, "Empresa no encontrada");
            
            if (tenant.Activo)
                tenant.Desactivar();
            else
                tenant.Activar();
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return new ToggleTenantResult(true, tenant.Activo);
        }
        catch (Exception ex)
        {
            return new ToggleTenantResult(false, false, ex.Message);
        }
    }
}

#endregion
