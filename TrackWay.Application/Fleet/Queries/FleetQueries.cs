using MediatR;
using TrackWay.Domain.Enums;

namespace TrackWay.Application.Fleet.Queries;

// ==================== Vehicle Queries ====================

/// <summary>
/// Query para obtener todos los vehículos activos
/// </summary>
public record GetAllVehiclesQuery() : IRequest<IReadOnlyList<VehicleDto>>;

/// <summary>
/// Query para obtener vehículo por ID
/// </summary>
public record GetVehicleByIdQuery(int Id) : IRequest<VehicleDetailDto?>;

/// <summary>
/// Query para obtener vehículos con alertas de documentos
/// </summary>
public record GetVehiclesWithAlertsQuery(int DiasAlerta = 30) : IRequest<IReadOnlyList<VehicleAlertDto>>;

/// <summary>
/// Query para obtener vehículos por estado
/// </summary>
public record GetVehiclesByEstadoQuery(EstadoOperativo Estado) : IRequest<IReadOnlyList<VehicleDto>>;

/// <summary>
/// Query para obtener vehículos disponibles para asignar
/// </summary>
public record GetAvailableVehiclesQuery() : IRequest<IReadOnlyList<VehicleDto>>;

// ==================== Driver Queries ====================

/// <summary>
/// Query para obtener todos los conductores activos
/// </summary>
public record GetAllDriversQuery() : IRequest<IReadOnlyList<DriverDto>>;

/// <summary>
/// Query para obtener conductor por ID
/// </summary>
public record GetDriverByIdQuery(int Id) : IRequest<DriverDetailDto?>;

/// <summary>
/// Query para obtener conductores con licencia por vencer
/// </summary>
public record GetDriversWithLicenseAlertsQuery(int DiasAlerta = 30) : IRequest<IReadOnlyList<DriverAlertDto>>;

/// <summary>
/// Query para obtener conductores disponibles
/// </summary>
public record GetAvailableDriversQuery() : IRequest<IReadOnlyList<DriverDto>>;

// ==================== Dashboard Queries ====================

/// <summary>
/// Query para obtener resumen del dashboard de Fleet
/// </summary>
public record GetFleetDashboardQuery() : IRequest<FleetDashboardDto>;

/// <summary>
/// Query para obtener todas las alertas activas
/// </summary>
public record GetAllAlertsQuery() : IRequest<IReadOnlyList<AlertDto>>;

// ==================== DTOs ====================

public record VehicleDto(
    int Id,
    string Placa,
    string Marca,
    string Modelo,
    string Estado,
    decimal KmActual,
    string? ConductorNombre);

public record VehicleDetailDto(
    int Id,
    string Placa,
    string VIN,
    string Marca,
    string Modelo,
    int AnoFabricacion,
    string Combustible,
    string Estado,
    decimal KmActual,
    string? ConductorNombre,
    int? ConductorId,
    IReadOnlyList<DocumentoResumenDto> Documentos,
    IReadOnlyList<MantenimientoResumenDto> MantenimientosPendientes);

public record VehicleAlertDto(
    int Id,
    string Placa,
    string Marca,
    string AlertaColor,
    string AlertaDescripcion,
    int DiasParaAlerta);

public record DriverDto(
    int Id,
    string NombreCompleto,
    string LicenciaNum,
    string CategoriaLicencia,
    decimal ScoringSeguridad,
    string AlertaColor);

public record DriverDetailDto(
    int Id,
    string Nombre,
    string Apellidos,
    string Documento,
    string Email,
    string Telefono,
    string LicenciaNum,
    string CategoriaLicencia,
    DateTime LicenciaVencimiento,
    int DiasParaVencimiento,
    decimal ScoringSeguridad,
    int TotalViajes,
    decimal KmTotales,
    string? VehiculoAsignado,
    string AlertaLicencia,
    string AlertaSeguridad);

public record DriverAlertDto(
    int Id,
    string NombreCompleto,
    DateTime LicenciaVencimiento,
    int DiasRestantes,
    string AlertaColor);

public record DocumentoResumenDto(
    int Id,
    string Tipo,
    DateTime FechaVencimiento,
    int DiasRestantes,
    string Alerta);

public record MantenimientoResumenDto(
    int Id,
    string Tipo,
    DateTime FechaProgramada,
    string Estado);

public record FleetDashboardDto(
    int TotalVehiculos,
    int VehiculosActivos,
    int VehiculosEnMantenimiento,
    int VehiculosEnRuta,
    int TotalConductores,
    int ConductoresDisponibles,
    int LicenciasPorVencer,
    int DocumentosPorVencer,
    int MantenimientosPendientes,
    decimal GastoCombustibleMes);

public record AlertDto(
    string Tipo,
    string Entidad,
    string Mensaje,
    string Color,
    DateTime? FechaVencimiento,
    int? DiasRestantes);
