using MediatR;
using TrackWay.Application.Fleet.Commands;
using TrackWay.Domain.Entities.Fleet;
using TrackWay.Domain.Interfaces;

namespace TrackWay.Application.Fleet.Handlers;

/// <summary>
/// Handler para crear vehículo
/// </summary>
public class CreateVehicleHandler : IRequestHandler<CreateVehicleCommand, CreateVehicleResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateVehicleHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateVehicleResult> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar placa única
            var existingVehicle = await _unitOfWork.Repository<Vehicle>()
                .GetFirstOrDefaultAsync(new Domain.Specifications.VehicleByPlacaSpec(request.Placa), cancellationToken);

            if (existingVehicle != null)
                return new CreateVehicleResult(0, request.Placa, false, "Ya existe un vehículo con esa placa");

            var vehicle = Vehicle.Create(
                request.Placa,
                request.VIN,
                request.Marca,
                request.Modelo,
                request.AnoFabricacion,
                request.Combustible,
                request.KmInicial);

            _unitOfWork.Repository<Vehicle>().Add(vehicle);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateVehicleResult(vehicle.Id, vehicle.Placa, true);
        }
        catch (Exception ex)
        {
            return new CreateVehicleResult(0, request.Placa, false, ex.Message);
        }
    }
}

/// <summary>
/// Handler para crear conductor
/// </summary>
public class CreateDriverHandler : IRequestHandler<CreateDriverCommand, CreateDriverResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateDriverHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateDriverResult> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var driver = Driver.Create(
                request.Nombre,
                request.Apellidos,
                request.Documento,
                request.LicenciaNum,
                request.CategoriaLicencia,
                request.LicenciaVencimiento);

            _unitOfWork.Repository<Driver>().Add(driver);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateDriverResult(driver.Id, driver.NombreCompleto, true);
        }
        catch (Exception ex)
        {
            return new CreateDriverResult(0, "", false, ex.Message);
        }
    }
}

/// <summary>
/// Handler para actualizar kilometraje
/// </summary>
public class UpdateVehicleKilometrajeHandler : IRequestHandler<UpdateVehicleKilometrajeCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVehicleKilometrajeHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateVehicleKilometrajeCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(request.VehicleId, cancellationToken);
        if (vehicle == null) return false;

        vehicle.ActualizarKilometraje(request.NuevoKm);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Handler para cambiar estado de vehículo
/// </summary>
public class ChangeVehicleEstadoHandler : IRequestHandler<ChangeVehicleEstadoCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ChangeVehicleEstadoHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ChangeVehicleEstadoCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(request.VehicleId, cancellationToken);
        if (vehicle == null) return false;

        vehicle.CambiarEstado(request.NuevoEstado);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Handler para dar de baja (desactivar) vehículo
/// </summary>
public class DeactivateVehicleHandler : IRequestHandler<DeactivateVehicleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateVehicleHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeactivateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(request.VehicleId, cancellationToken);
        if (vehicle == null) return false;

        vehicle.DarDeBaja();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
