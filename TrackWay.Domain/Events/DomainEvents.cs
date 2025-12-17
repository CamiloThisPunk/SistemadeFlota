using TrackWay.Domain.Common;

namespace TrackWay.Domain.Events;

/// <summary>
/// Evento base de dominio
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// ============ Vehicle Events ============

public sealed record VehicleCreatedEvent(int VehicleId, string Placa, string Marca, string Modelo) : DomainEventBase;

public sealed record VehicleStatusChangedEvent(int VehicleId, string OldStatus, string NewStatus) : DomainEventBase;

public sealed record VehicleKilometrageUpdatedEvent(int VehicleId, decimal OldKm, decimal NewKm) : DomainEventBase;

// ============ Driver Events ============

public sealed record DriverCreatedEvent(int DriverId, string Nombre, string LicenciaNum) : DomainEventBase;

public sealed record DriverLicenseExpiringEvent(int DriverId, string Nombre, DateTime ExpiryDate, int DaysRemaining) : DomainEventBase;

public sealed record DriverScoreUpdatedEvent(int DriverId, decimal OldScore, decimal NewScore) : DomainEventBase;

// ============ Maintenance Events ============

public sealed record MaintenanceOrderCreatedEvent(int OrderId, int VehicleId, string Tipo) : DomainEventBase;

public sealed record MaintenanceOrderCompletedEvent(int OrderId, int VehicleId, decimal Costo) : DomainEventBase;

public sealed record MaintenanceOverdueEvent(int OrderId, int VehicleId, DateTime ScheduledDate) : DomainEventBase;

// ============ Fuel Events ============

public sealed record FuelLoadRegisteredEvent(int FuelLoadId, int VehicleId, decimal Galones, decimal Precio) : DomainEventBase;

public sealed record AbnormalFuelConsumptionEvent(int VehicleId, decimal ExpectedConsumption, decimal ActualConsumption) : DomainEventBase;

// ============ Document Events ============

public sealed record DocumentExpiringEvent(int DocumentId, int VehicleId, string TipoDocumento, DateTime ExpiryDate, int DaysRemaining) : DomainEventBase;

public sealed record DocumentExpiredEvent(int DocumentId, int VehicleId, string TipoDocumento) : DomainEventBase;
