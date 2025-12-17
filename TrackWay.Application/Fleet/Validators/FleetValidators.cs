using FluentValidation;
using TrackWay.Application.Fleet.Commands;

namespace TrackWay.Application.Fleet.Validators;

/// <summary>
/// Validator para CreateVehicleCommand
/// </summary>
public class CreateVehicleValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleValidator()
    {
        RuleFor(x => x.Placa)
            .NotEmpty().WithMessage("La placa es requerida")
            .MaximumLength(20).WithMessage("La placa no puede exceder 20 caracteres")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("La placa solo puede contener letras, números y guiones");

        RuleFor(x => x.VIN)
            .MaximumLength(50).WithMessage("El VIN no puede exceder 50 caracteres");

        RuleFor(x => x.Marca)
            .NotEmpty().WithMessage("La marca es requerida")
            .MaximumLength(50).WithMessage("La marca no puede exceder 50 caracteres");

        RuleFor(x => x.Modelo)
            .NotEmpty().WithMessage("El modelo es requerido")
            .MaximumLength(50).WithMessage("El modelo no puede exceder 50 caracteres");

        RuleFor(x => x.AnoFabricacion)
            .InclusiveBetween(1900, DateTime.Now.Year + 1)
            .WithMessage($"El año debe estar entre 1900 y {DateTime.Now.Year + 1}");

        RuleFor(x => x.KmInicial)
            .GreaterThanOrEqualTo(0).WithMessage("El kilometraje no puede ser negativo");
    }
}

/// <summary>
/// Validator para CreateDriverCommand
/// </summary>
public class CreateDriverValidator : AbstractValidator<CreateDriverCommand>
{
    public CreateDriverValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos son requeridos")
            .MaximumLength(100).WithMessage("Los apellidos no pueden exceder 100 caracteres");

        RuleFor(x => x.Documento)
            .NotEmpty().WithMessage("El documento es requerido")
            .MaximumLength(20).WithMessage("El documento no puede exceder 20 caracteres");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("El email no es válido");

        RuleFor(x => x.LicenciaNum)
            .NotEmpty().WithMessage("El número de licencia es requerido")
            .MaximumLength(30).WithMessage("El número de licencia no puede exceder 30 caracteres");

        RuleFor(x => x.LicenciaVencimiento)
            .GreaterThan(DateTime.UtcNow).WithMessage("La fecha de vencimiento debe ser futura");
    }
}

/// <summary>
/// Validator para UpdateVehicleKilometrajeCommand
/// </summary>
public class UpdateKilometrajeValidator : AbstractValidator<UpdateVehicleKilometrajeCommand>
{
    public UpdateKilometrajeValidator()
    {
        RuleFor(x => x.VehicleId)
            .GreaterThan(0).WithMessage("El ID del vehículo es requerido");

        RuleFor(x => x.NuevoKm)
            .GreaterThanOrEqualTo(0).WithMessage("El kilometraje no puede ser negativo");
    }
}

/// <summary>
/// Validator para UpdateDriverScoringCommand
/// </summary>
public class UpdateDriverScoringValidator : AbstractValidator<UpdateDriverScoringCommand>
{
    public UpdateDriverScoringValidator()
    {
        RuleFor(x => x.DriverId)
            .GreaterThan(0).WithMessage("El ID del conductor es requerido");

        RuleFor(x => x.NuevoScore)
            .InclusiveBetween(0, 100).WithMessage("El score debe estar entre 0 y 100");
    }
}

/// <summary>
/// Validator para CreateMaintenanceOrderCommand
/// </summary>
public class CreateMaintenanceValidator : AbstractValidator<CreateMaintenanceOrderCommand>
{
    public CreateMaintenanceValidator()
    {
        RuleFor(x => x.VehicleId)
            .GreaterThan(0).WithMessage("El vehículo es requerido");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida")
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres");

        RuleFor(x => x.FechaProgramada)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("La fecha programada no puede ser pasada");

        RuleFor(x => x.CostoEstimado)
            .GreaterThanOrEqualTo(0).WithMessage("El costo estimado no puede ser negativo");
    }
}
