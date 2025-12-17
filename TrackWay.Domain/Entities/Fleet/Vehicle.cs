using TrackWay.Domain.Common;
using TrackWay.Domain.Enums;
using TrackWay.Domain.Events;
using TrackWay.Domain.ValueObjects;

namespace TrackWay.Domain.Entities.Fleet;

/// <summary>
/// Entidad principal de Vehículo - Aggregate Root
/// </summary>
public class Vehicle : AggregateRoot
{
    // Identificación
    public string Placa { get; private set; } = string.Empty;
    public string VIN { get; private set; } = string.Empty;
    
    // Características
    public string Marca { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public int AnoFabricacion { get; private set; }
    public string Color { get; private set; } = string.Empty;
    
    // Operativo
    public TipoCombustible Combustible { get; private set; }
    public EstadoOperativo Estado { get; private set; }
    public decimal KmActual { get; private set; }
    public decimal CapacidadTanque { get; private set; }
    
    // Auditoría
    public DateTime FechaRegistro { get; private set; }
    public DateTime? FechaUltimaActualizacion { get; private set; }
    public bool Activo { get; private set; } = true;
    
    // Relaciones
    public int? ConductorAsignadoId { get; private set; }
    public Driver? ConductorAsignado { get; private set; }
    
    private readonly List<MaintenanceOrder> _ordenesMantenimiento = new();
    public IReadOnlyCollection<MaintenanceOrder> OrdenesMantenimiento => _ordenesMantenimiento.AsReadOnly();
    
    private readonly List<FuelLoad> _cargasCombustible = new();
    public IReadOnlyCollection<FuelLoad> CargasCombustible => _cargasCombustible.AsReadOnly();
    
    private readonly List<VehicleDocument> _documentos = new();
    public IReadOnlyCollection<VehicleDocument> Documentos => _documentos.AsReadOnly();
    
    // Constructor privado para EF Core
    private Vehicle() { }
    
    // Factory method
    public static Vehicle Create(
        string placa,
        string vin,
        string marca,
        string modelo,
        int anoFabricacion,
        TipoCombustible combustible,
        decimal kmInicial = 0)
    {
        if (string.IsNullOrWhiteSpace(placa))
            throw new ArgumentException("La placa es requerida", nameof(placa));
        
        if (anoFabricacion < 1900 || anoFabricacion > DateTime.Now.Year + 1)
            throw new ArgumentException("Año de fabricación inválido", nameof(anoFabricacion));
        
        var vehicle = new Vehicle
        {
            Placa = placa.ToUpper().Trim(),
            VIN = vin?.ToUpper().Trim() ?? string.Empty,
            Marca = marca,
            Modelo = modelo,
            AnoFabricacion = anoFabricacion,
            Combustible = combustible,
            Estado = EstadoOperativo.Activo,
            KmActual = kmInicial,
            FechaRegistro = DateTime.UtcNow
        };
        
        vehicle.AddDomainEvent(new VehicleCreatedEvent(vehicle.Id, placa, marca, modelo));
        
        return vehicle;
    }
    
    // Métodos de dominio
    public void ActualizarKilometraje(decimal nuevoKm)
    {
        if (nuevoKm < KmActual)
            throw new InvalidOperationException("El nuevo kilometraje no puede ser menor al actual");
        
        var oldKm = KmActual;
        KmActual = nuevoKm;
        FechaUltimaActualizacion = DateTime.UtcNow;
        
        AddDomainEvent(new VehicleKilometrageUpdatedEvent(Id, oldKm, nuevoKm));
    }
    
    public void CambiarEstado(EstadoOperativo nuevoEstado)
    {
        if (Estado == nuevoEstado) return;
        
        var oldEstado = Estado.ToString();
        Estado = nuevoEstado;
        FechaUltimaActualizacion = DateTime.UtcNow;
        
        AddDomainEvent(new VehicleStatusChangedEvent(Id, oldEstado, nuevoEstado.ToString()));
    }
    
    public void AsignarConductor(Driver conductor)
    {
        ConductorAsignadoId = conductor.Id;
        ConductorAsignado = conductor;
        FechaUltimaActualizacion = DateTime.UtcNow;
    }
    
    public void DesasignarConductor()
    {
        ConductorAsignadoId = null;
        ConductorAsignado = null;
        FechaUltimaActualizacion = DateTime.UtcNow;
    }
    
    public void EnviarAMantenimiento()
    {
        CambiarEstado(EstadoOperativo.EnMantenimiento);
        DesasignarConductor();
    }
    
    public void DarDeBaja()
    {
        CambiarEstado(EstadoOperativo.BajaDefinitiva);
        Activo = false;
        DesasignarConductor();
    }
    
    public AlertStatus GetAlertaDocumentos()
    {
        if (!_documentos.Any())
            return AlertStatus.Rojo;
        
        var diasMinimoVencimiento = _documentos
            .Where(d => d.Activo)
            .Select(d => (d.FechaVencimiento - DateTime.UtcNow).Days)
            .DefaultIfEmpty(int.MaxValue)
            .Min();
        
        return AlertStatus.FromDaysToExpiry(diasMinimoVencimiento);
    }
    
    public decimal CalcularConsumoPromedio()
    {
        if (_cargasCombustible.Count < 2) return 0;
        
        var cargas = _cargasCombustible.OrderBy(c => c.Fecha).ToList();
        var totalGalones = cargas.Sum(c => c.Galones);
        var kmRecorridos = cargas.Last().KmOdometro - cargas.First().KmOdometro;
        
        return kmRecorridos > 0 ? totalGalones / kmRecorridos * 100 : 0;
    }
}
