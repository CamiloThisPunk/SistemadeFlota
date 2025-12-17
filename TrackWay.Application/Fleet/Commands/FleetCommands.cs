using MediatR;
using TrackWay.Domain.Enums;

namespace TrackWay.Application.Fleet.Commands;

// ==================== Vehicle Commands ====================

/// <summary>
/// Comando para crear un vehículo
/// </summary>
public record CreateVehicleCommand(
    string Placa,
    string VIN,
    string Marca,
    string Modelo,
    int AnoFabricacion,
    TipoCombustible Combustible,
    decimal KmInicial = 0
) : IRequest<CreateVehicleResult>;

public record CreateVehicleResult(int Id, string Placa, bool Success, string? Error = null);

/// <summary>
/// Comando para actualizar kilometraje
/// </summary>
public record UpdateVehicleKilometrajeCommand(int VehicleId, decimal NuevoKm) : IRequest<bool>;

/// <summary>
/// Comando para cambiar estado de vehículo
/// </summary>
public record ChangeVehicleEstadoCommand(int VehicleId, EstadoOperativo NuevoEstado) : IRequest<bool>;

/// <summary>
/// Comando para asignar conductor a vehículo
/// </summary>
public record AssignDriverToVehicleCommand(int VehicleId, int DriverId) : IRequest<bool>;

/// <summary>
/// Comando para desasignar conductor
/// </summary>
public record UnassignDriverCommand(int VehicleId) : IRequest<bool>;

/// <summary>
/// Comando para dar de baja vehículo
/// </summary>
public record DeactivateVehicleCommand(int VehicleId) : IRequest<bool>;

// ==================== Driver Commands ====================

/// <summary>
/// Comando para crear un conductor
/// </summary>
public record CreateDriverCommand(
    string Nombre,
    string Apellidos,
    string Documento,
    string Email,
    string Telefono,
    string LicenciaNum,
    CategoriaLicencia CategoriaLicencia,
    DateTime LicenciaVencimiento
) : IRequest<CreateDriverResult>;

public record CreateDriverResult(int Id, string NombreCompleto, bool Success, string? Error = null);

/// <summary>
/// Comando para actualizar scoring de seguridad
/// </summary>
public record UpdateDriverScoringCommand(int DriverId, decimal NuevoScore) : IRequest<bool>;

/// <summary>
/// Comando para renovar licencia
/// </summary>
public record RenewDriverLicenseCommand(int DriverId, DateTime NuevaFechaVencimiento) : IRequest<bool>;

// ==================== Maintenance Commands ====================

/// <summary>
/// Comando para crear orden de mantenimiento
/// </summary>
public record CreateMaintenanceOrderCommand(
    int VehicleId,
    TipoMantenimiento Tipo,
    string Descripcion,
    DateTime FechaProgramada,
    decimal CostoEstimado
) : IRequest<int>;

/// <summary>
/// Comando para finalizar mantenimiento
/// </summary>
public record CompleteMaintenanceCommand(
    int OrderId,
    decimal CostoReal,
    string? Observaciones
) : IRequest<bool>;

// ==================== Document Commands ====================

/// <summary>
/// Comando para agregar documento a vehículo
/// </summary>
public record AddVehicleDocumentCommand(
    int VehicleId,
    TipoDocumento Tipo,
    string NumeroDocumento,
    DateTime FechaEmision,
    DateTime FechaVencimiento,
    string? Emisor,
    decimal? Costo
) : IRequest<int>;

/// <summary>
/// Comando para renovar documento
/// </summary>
public record RenewDocumentCommand(
    int DocumentId,
    DateTime NuevaFechaVencimiento,
    string? NuevoNumero
) : IRequest<bool>;
