namespace TrackWay.Domain.DTOs;

/// <summary>
/// DTOs internos inmutables usando records
/// </summary>

// Vehicle DTOs
public sealed record VehicleBasicDto(
    int Id,
    string Placa,
    string Marca,
    string Modelo,
    string Estado,
    decimal KmActual);

public sealed record VehicleDetailDto(
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
    int DocumentosVigentes,
    int MantenimientosPendientes,
    string AlertaDocumentos);

public sealed record VehicleCreateDto(
    string Placa,
    string VIN,
    string Marca,
    string Modelo,
    int AnoFabricacion,
    string Combustible,
    decimal KmInicial);

// Driver DTOs
public sealed record DriverBasicDto(
    int Id,
    string NombreCompleto,
    string LicenciaNum,
    decimal ScoringSeguridad,
    string AlertaLicencia);

public sealed record DriverDetailDto(
    int Id,
    string Nombre,
    string Apellidos,
    string Documento,
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

public sealed record DriverCreateDto(
    string Nombre,
    string Apellidos,
    string Documento,
    string LicenciaNum,
    string CategoriaLicencia,
    DateTime LicenciaVencimiento);

// Maintenance DTOs
public sealed record MaintenanceOrderDto(
    int Id,
    int VehicleId,
    string VehiculoPlaca,
    string Tipo,
    string Estado,
    string Descripcion,
    DateTime FechaProgramada,
    decimal KmAlMomento,
    decimal CostoEstimado,
    decimal CostoReal,
    string? Proveedor,
    string Alerta);

public sealed record MaintenanceCreateDto(
    int VehicleId,
    string Tipo,
    string Descripcion,
    DateTime FechaProgramada,
    decimal CostoEstimado);

// FuelLoad DTOs
public sealed record FuelLoadDto(
    int Id,
    int VehicleId,
    string VehiculoPlaca,
    DateTime Fecha,
    decimal Galones,
    decimal PrecioPorGalon,
    decimal Total,
    decimal KmOdometro,
    string? Estacion,
    string? ConductorNombre);

public sealed record FuelLoadCreateDto(
    int VehicleId,
    decimal Galones,
    decimal PrecioPorGalon,
    decimal KmOdometro,
    string? Estacion,
    int? ConductorId);

// Document DTOs
public sealed record VehicleDocumentDto(
    int Id,
    int VehicleId,
    string Tipo,
    string NumeroDocumento,
    DateTime FechaEmision,
    DateTime FechaVencimiento,
    int DiasParaVencimiento,
    bool EstaVigente,
    string? ArchivoUrl,
    string Alerta);

public sealed record DocumentCreateDto(
    int VehicleId,
    string Tipo,
    string NumeroDocumento,
    DateTime FechaEmision,
    DateTime FechaVencimiento,
    string? Emisor,
    decimal? Costo);

// Dashboard DTOs
public sealed record FleetDashboardDto(
    int TotalVehiculos,
    int VehiculosActivos,
    int VehiculosEnMantenimiento,
    int TotalConductores,
    int ConductoresDisponibles,
    int LicenciasPorVencer,
    int DocumentosPorVencer,
    int MantenimientosPendientes,
    decimal GastoCombustibleMes);

public sealed record AlertaSummaryDto(
    string Tipo,
    string Mensaje,
    string Alerta,
    int EntidadId,
    DateTime? Fecha);
