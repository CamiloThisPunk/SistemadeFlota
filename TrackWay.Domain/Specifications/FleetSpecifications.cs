using TrackWay.Domain.Entities.Fleet;
using TrackWay.Domain.Enums;
using TrackWay.Domain.Interfaces;

namespace TrackWay.Domain.Specifications;

/// <summary>
/// Especificación para vehículos activos
/// </summary>
public class VehiclesActivosSpec : BaseSpecification<Vehicle>
{
    public VehiclesActivosSpec() : base(v => v.Activo)
    {
        AddInclude(v => v.ConductorAsignado!);
        ApplyOrderBy(v => v.Placa);
    }
}

/// <summary>
/// Especificación para vehículos con alertas de documentos
/// </summary>
public class VehiclesConAlertasDocumentosSpec : BaseSpecification<Vehicle>
{
    public VehiclesConAlertasDocumentosSpec(int diasAlerta = 30) 
        : base(v => v.Activo && v.Documentos.Any(d => 
            d.Activo && d.FechaVencimiento <= DateTime.UtcNow.AddDays(diasAlerta)))
    {
        AddInclude(v => v.Documentos);
        ApplyOrderBy(v => v.Placa);
    }
}

/// <summary>
/// Especificación para vehículos por estado
/// </summary>
public class VehiclesByEstadoSpec : BaseSpecification<Vehicle>
{
    public VehiclesByEstadoSpec(EstadoOperativo estado) 
        : base(v => v.Activo && v.Estado == estado)
    {
        AddInclude(v => v.ConductorAsignado!);
    }
}

/// <summary>
/// Especificación para vehículo por placa
/// </summary>
public class VehicleByPlacaSpec : BaseSpecification<Vehicle>
{
    public VehicleByPlacaSpec(string placa) 
        : base(v => v.Placa == placa.ToUpper())
    {
        AddInclude(v => v.ConductorAsignado!);
        AddInclude(v => v.Documentos);
        AddInclude(v => v.OrdenesMantenimiento);
    }
}

/// <summary>
/// Especificación para conductores con licencia por vencer
/// </summary>
public class DriversLicenciaPorVencerSpec : BaseSpecification<Driver>
{
    public DriversLicenciaPorVencerSpec(int diasAlerta = 30) 
        : base(d => d.Activo && d.LicenciaVencimiento <= DateTime.UtcNow.AddDays(diasAlerta))
    {
        ApplyOrderBy(d => d.LicenciaVencimiento);
    }
}

/// <summary>
/// Especificación para conductores disponibles
/// </summary>
public class DriversDisponiblesSpec : BaseSpecification<Driver>
{
    public DriversDisponiblesSpec() 
        : base(d => d.Activo && d.LicenciaVencimiento > DateTime.UtcNow && d.VehiculoAsignadoId == null)
    {
        ApplyOrderBy(d => d.Nombre);
    }
}

/// <summary>
/// Especificación para mantenimientos pendientes
/// </summary>
public class MaintenancePendientesSpec : BaseSpecification<MaintenanceOrder>
{
    public MaintenancePendientesSpec() 
        : base(m => m.Estado == EstadoMantenimiento.Pendiente || m.Estado == EstadoMantenimiento.Programado)
    {
        AddInclude(m => m.Vehicle!);
        ApplyOrderBy(m => m.FechaProgramada);
    }
}

/// <summary>
/// Especificación para documentos por vencer
/// </summary>
public class DocumentosPorVencerSpec : BaseSpecification<VehicleDocument>
{
    public DocumentosPorVencerSpec(int diasAlerta = 30) 
        : base(d => d.Activo && d.FechaVencimiento <= DateTime.UtcNow.AddDays(diasAlerta))
    {
        AddInclude(d => d.Vehicle!);
        ApplyOrderBy(d => d.FechaVencimiento);
    }
}
