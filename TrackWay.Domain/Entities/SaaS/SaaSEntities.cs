using TrackWay.Domain.Common;
using TrackWay.Domain.Enums;

namespace TrackWay.Domain.Entities.SaaS;

/// <summary>
/// Plan de suscripción SaaS (Free, Pro, Enterprise)
/// </summary>
public class SubscriptionPlan : Entity
{
    public string Nombre { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public decimal PrecioMensual { get; private set; }
    public int MaxUsuarios { get; private set; }
    public int MaxVehiculos { get; private set; }
    public PlanTier Tier { get; private set; }
    public bool Activo { get; private set; } = true;
    
    // Color para UI (hex)
    public string ColorHex { get; private set; } = "#6c5ce7";
    
    // Navegación
    private readonly List<TenantSubscription> _subscriptions = new();
    public IReadOnlyCollection<TenantSubscription> Subscriptions => _subscriptions.AsReadOnly();
    
    private SubscriptionPlan() { }
    
    public static SubscriptionPlan Create(
        string nombre,
        string descripcion,
        decimal precioMensual,
        int maxUsuarios,
        int maxVehiculos,
        PlanTier tier,
        string colorHex = "#6c5ce7")
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del plan es requerido");
        if (maxUsuarios < 1)
            throw new ArgumentException("El plan debe permitir al menos 1 usuario");
        if (maxVehiculos < 1)
            throw new ArgumentException("El plan debe permitir al menos 1 vehículo");
        
        return new SubscriptionPlan
        {
            Nombre = nombre,
            Descripcion = descripcion,
            PrecioMensual = precioMensual,
            MaxUsuarios = maxUsuarios,
            MaxVehiculos = maxVehiculos,
            Tier = tier,
            ColorHex = colorHex
        };
    }
    
    public void Actualizar(string nombre, string descripcion, decimal precio, int maxUsuarios, int maxVehiculos)
    {
        Nombre = nombre;
        Descripcion = descripcion;
        PrecioMensual = precio;
        MaxUsuarios = maxUsuarios;
        MaxVehiculos = maxVehiculos;
    }
    
    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}

/// <summary>
/// Empresa/Inquilino en el modelo SaaS multi-tenant
/// </summary>
public class Tenant : AggregateRoot
{
    public string Nombre { get; private set; } = string.Empty;
    public string RUC { get; private set; } = string.Empty;
    public string EmailContacto { get; private set; } = string.Empty;
    public string? Telefono { get; private set; }
    public string? Direccion { get; private set; }
    public string? LogoUrl { get; private set; }
    
    public bool Activo { get; private set; } = true;
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaDesactivacion { get; private set; }
    
    // Métricas agregadas (actualizadas periódicamente)
    public int TotalUsuarios { get; private set; }
    public int TotalVehiculos { get; private set; }
    public int TotalMantenimientos { get; private set; }
    
    // Navegación
    private readonly List<TenantSubscription> _subscriptions = new();
    public IReadOnlyCollection<TenantSubscription> Subscriptions => _subscriptions.AsReadOnly();
    
    // Suscripción activa actual
    public TenantSubscription? SuscripcionActiva => _subscriptions.FirstOrDefault(s => s.EstaActiva);
    
    private Tenant() { }
    
    public static Tenant Create(
        string nombre,
        string ruc,
        string emailContacto,
        string? telefono = null,
        string? direccion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la empresa es requerido");
        if (string.IsNullOrWhiteSpace(ruc))
            throw new ArgumentException("El RUC es requerido");
        if (string.IsNullOrWhiteSpace(emailContacto))
            throw new ArgumentException("El email de contacto es requerido");
        
        return new Tenant
        {
            Nombre = nombre,
            RUC = ruc.Trim(),
            EmailContacto = emailContacto.ToLower().Trim(),
            Telefono = telefono,
            Direccion = direccion,
            FechaCreacion = DateTime.UtcNow
        };
    }
    
    public void ActualizarDatos(string nombre, string emailContacto, string? telefono, string? direccion)
    {
        Nombre = nombre;
        EmailContacto = emailContacto.ToLower().Trim();
        Telefono = telefono;
        Direccion = direccion;
    }
    
    public void ActualizarMetricas(int usuarios, int vehiculos, int mantenimientos)
    {
        TotalUsuarios = usuarios;
        TotalVehiculos = vehiculos;
        TotalMantenimientos = mantenimientos;
    }
    
    public void Desactivar()
    {
        Activo = false;
        FechaDesactivacion = DateTime.UtcNow;
    }
    
    public void Activar()
    {
        Activo = true;
        FechaDesactivacion = null;
    }
    
    public void AsignarSuscripcion(SubscriptionPlan plan)
    {
        // Finalizar suscripción actual si existe
        var suscripcionActual = SuscripcionActiva;
        if (suscripcionActual != null)
        {
            suscripcionActual.Finalizar();
        }
        
        // Crear nueva suscripción
        var nuevaSuscripcion = TenantSubscription.Create(this.Id, plan.Id);
        _subscriptions.Add(nuevaSuscripcion);
    }
    
    public void SetLogoUrl(string? logoUrl) => LogoUrl = logoUrl;
    
    // Verificar si está cerca del límite
    public bool CercaDelLimiteUsuarios(int porcentajeAlerta = 90)
    {
        var plan = SuscripcionActiva?.Plan;
        if (plan == null) return false;
        return (TotalUsuarios * 100.0 / plan.MaxUsuarios) >= porcentajeAlerta;
    }
    
    public bool CercaDelLimiteVehiculos(int porcentajeAlerta = 90)
    {
        var plan = SuscripcionActiva?.Plan;
        if (plan == null) return false;
        return (TotalVehiculos * 100.0 / plan.MaxVehiculos) >= porcentajeAlerta;
    }
    
    /// <summary>
    /// Para uso en seeder - simula datos de actividad del tenant
    /// </summary>
    public void SimularActividad(int usuarios, int vehiculos, int mantenimientos)
    {
        TotalUsuarios = usuarios;
        TotalVehiculos = vehiculos;
        TotalMantenimientos = mantenimientos;
    }
}

/// <summary>
/// Vincula un Tenant con su plan de suscripción
/// </summary>
public class TenantSubscription : Entity
{
    public int TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;
    
    public int SubscriptionPlanId { get; private set; }
    public SubscriptionPlan Plan { get; private set; } = null!;
    
    public DateTime FechaInicio { get; private set; }
    public DateTime? FechaFin { get; private set; }
    
    public bool EstaActiva => FechaFin == null || FechaFin > DateTime.UtcNow;
    
    private TenantSubscription() { }
    
    public static TenantSubscription Create(int tenantId, int planId)
    {
        return new TenantSubscription
        {
            TenantId = tenantId,
            SubscriptionPlanId = planId,
            FechaInicio = DateTime.UtcNow
        };
    }
    
    public void Finalizar()
    {
        FechaFin = DateTime.UtcNow;
    }
    
    public void Renovar()
    {
        FechaFin = null;
    }
}
