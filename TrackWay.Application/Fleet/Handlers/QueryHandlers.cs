using MediatR;
using TrackWay.Application.Fleet.Queries;
using TrackWay.Domain.Entities.Fleet;
using TrackWay.Domain.Enums;
using TrackWay.Domain.Interfaces;
using TrackWay.Domain.Specifications;

namespace TrackWay.Application.Fleet.Handlers;

/// <summary>
/// Handler para obtener todos los vehículos
/// </summary>
public class GetAllVehiclesHandler : IRequestHandler<GetAllVehiclesQuery, IReadOnlyList<VehicleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllVehiclesHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<VehicleDto>> Handle(GetAllVehiclesQuery request, CancellationToken cancellationToken)
    {
        var vehicles = await _unitOfWork.Repository<Vehicle>()
            .GetAsync(new VehiclesActivosSpec(), cancellationToken);

        return vehicles.Select(v => new VehicleDto(
            v.Id,
            v.Placa,
            v.Marca,
            v.Modelo,
            v.Estado.ToString(),
            v.KmActual,
            v.ConductorAsignado != null ? $"{v.ConductorAsignado.Nombre} {v.ConductorAsignado.Apellidos}" : null
        )).ToList();
    }
}

/// <summary>
/// Handler para obtener vehículos con alertas
/// </summary>
public class GetVehiclesWithAlertsHandler : IRequestHandler<GetVehiclesWithAlertsQuery, IReadOnlyList<VehicleAlertDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVehiclesWithAlertsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<VehicleAlertDto>> Handle(GetVehiclesWithAlertsQuery request, CancellationToken cancellationToken)
    {
        var vehicles = await _unitOfWork.Repository<Vehicle>()
            .GetAsync(new VehiclesConAlertasDocumentosSpec(request.DiasAlerta), cancellationToken);

        return vehicles.Select(v =>
        {
            var alerta = v.GetAlertaDocumentos();
            var docMasProximoVencer = v.Documentos
                .Where(d => d.Activo)
                .OrderBy(d => d.FechaVencimiento)
                .FirstOrDefault();

            return new VehicleAlertDto(
                v.Id,
                v.Placa,
                v.Marca,
                alerta.Color,
                alerta.Descripcion,
                docMasProximoVencer?.DiasParaVencimiento ?? 0
            );
        }).ToList();
    }
}

/// <summary>
/// Handler para obtener todos los conductores
/// </summary>
public class GetAllDriversHandler : IRequestHandler<GetAllDriversQuery, IReadOnlyList<DriverDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllDriversHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DriverDto>> Handle(GetAllDriversQuery request, CancellationToken cancellationToken)
    {
        var drivers = await _unitOfWork.Repository<Driver>().GetAllAsync(cancellationToken);

        return drivers
            .Where(d => d.Activo)
            .Select(d => new DriverDto(
                d.Id,
                d.Nombre,
                d.Apellidos,
                d.Documento,
                d.LicenciaNum,
                d.CategoriaLicencia.ToString(),
                d.LicenciaVencimiento,
                d.ScoringSeguridad,
                d.Activo,
                d.GetAlertaLicencia().Color
            )).ToList();
    }
}

/// <summary>
/// Handler para obtener conductores con alertas de licencia
/// </summary>
public class GetDriversWithLicenseAlertsHandler : IRequestHandler<GetDriversWithLicenseAlertsQuery, IReadOnlyList<DriverAlertDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDriversWithLicenseAlertsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DriverAlertDto>> Handle(GetDriversWithLicenseAlertsQuery request, CancellationToken cancellationToken)
    {
        var drivers = await _unitOfWork.Repository<Driver>()
            .GetAsync(new DriversLicenciaPorVencerSpec(request.DiasAlerta), cancellationToken);

        return drivers.Select(d => new DriverAlertDto(
            d.Id,
            d.NombreCompleto,
            d.LicenciaVencimiento,
            d.DiasParaVencimientoLicencia,
            d.GetAlertaLicencia().Color
        )).ToList();
    }
}

/// <summary>
/// Handler para obtener dashboard de Fleet
/// </summary>
public class GetFleetDashboardHandler : IRequestHandler<GetFleetDashboardQuery, FleetDashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFleetDashboardHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<FleetDashboardDto> Handle(GetFleetDashboardQuery request, CancellationToken cancellationToken)
    {
        var vehicles = await _unitOfWork.Repository<Vehicle>()
            .GetAsync(new VehiclesActivosSpec(), cancellationToken);
        var drivers = await _unitOfWork.Repository<Driver>()
            .GetAsync(new DriversDisponiblesSpec(), cancellationToken);
        var licenciasAlerta = await _unitOfWork.Repository<Driver>()
            .CountAsync(new DriversLicenciaPorVencerSpec(30), cancellationToken);
        var mantenimientosPendientes = await _unitOfWork.Repository<MaintenanceOrder>()
            .CountAsync(new MaintenancePendientesSpec(), cancellationToken);
        var documentosAlerta = await _unitOfWork.Repository<VehicleDocument>()
            .CountAsync(new DocumentosPorVencerSpec(30), cancellationToken);

        return new FleetDashboardDto(
            TotalVehiculos: vehicles.Count,
            VehiculosActivos: vehicles.Count(v => v.Estado == EstadoOperativo.Activo),
            VehiculosEnMantenimiento: vehicles.Count(v => v.Estado == EstadoOperativo.EnMantenimiento),
            VehiculosEnRuta: vehicles.Count(v => v.Estado == EstadoOperativo.EnRuta),
            TotalConductores: drivers.Count,
            ConductoresDisponibles: drivers.Count,
            LicenciasPorVencer: licenciasAlerta,
            DocumentosPorVencer: documentosAlerta,
            MantenimientosPendientes: mantenimientosPendientes,
            GastoCombustibleMes: 0 // TODO: Calcular
        );
    }
}
