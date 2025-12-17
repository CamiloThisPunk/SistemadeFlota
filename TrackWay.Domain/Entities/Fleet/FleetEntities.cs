using TrackWay.Domain.Common;
using TrackWay.Domain.Enums;
using TrackWay.Domain.Events;
using TrackWay.Domain.ValueObjects;

namespace TrackWay.Domain.Entities.Fleet;

/// <summary>
/// Orden de Mantenimiento - Entity
/// </summary>
public class MaintenanceOrder : Entity
{
    public int VehicleId { get; private set; }
    public Vehicle? Vehicle { get; private set; }
    
    public TipoMantenimiento Tipo { get; private set; }
    public EstadoMantenimiento Estado { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    
    public DateTime FechaProgramada { get; private set; }
    public DateTime? FechaInicio { get; private set; }
    public DateTime? FechaFin { get; private set; }
    
    public decimal KmAlMomento { get; private set; }
    public decimal? KmProximoMantenimiento { get; private set; }
    
    public decimal CostoEstimado { get; private set; }
    public decimal CostoReal { get; private set; }
    
    public string? Proveedor { get; private set; }
    public string? Observaciones { get; private set; }
    
    public DateTime FechaCreacion { get; private set; }
    
    private MaintenanceOrder() { }
    
    public static MaintenanceOrder Create(
        int vehicleId,
        TipoMantenimiento tipo,
        string descripcion,
        DateTime fechaProgramada,
        decimal km,
        decimal costoEstimado = 0)
    {
        // Comparar solo fechas sin hora, permitiendo la fecha de hoy
        if (fechaProgramada.Date < DateTime.Today)
            throw new ArgumentException("La fecha programada no puede ser pasada");
        
        return new MaintenanceOrder
        {
            VehicleId = vehicleId,
            Tipo = tipo,
            Estado = EstadoMantenimiento.Pendiente,
            Descripcion = descripcion,
            FechaProgramada = fechaProgramada,
            KmAlMomento = km,
            CostoEstimado = costoEstimado,
            FechaCreacion = DateTime.UtcNow
        };
    }
    
    public void Programar(DateTime nuevaFecha)
    {
        if (Estado != EstadoMantenimiento.Pendiente)
            throw new InvalidOperationException("Solo se pueden reprogramar órdenes pendientes");
        
        Estado = EstadoMantenimiento.Programado;
        FechaProgramada = nuevaFecha;
    }
    
    public void IniciarTrabajo()
    {
        if (Estado != EstadoMantenimiento.Programado && Estado != EstadoMantenimiento.Pendiente)
            throw new InvalidOperationException("La orden no está lista para iniciar");
        
        Estado = EstadoMantenimiento.EnTaller;
        FechaInicio = DateTime.UtcNow;
    }
    
    public void Finalizar(decimal costoReal, string? observaciones = null)
    {
        if (Estado != EstadoMantenimiento.EnTaller && Estado != EstadoMantenimiento.EnEjecucion)
            throw new InvalidOperationException("La orden no está en ejecución");
        
        Estado = EstadoMantenimiento.Finalizado;
        FechaFin = DateTime.UtcNow;
        CostoReal = costoReal;
        Observaciones = observaciones;
    }
    
    public void Cancelar(string motivo)
    {
        if (Estado == EstadoMantenimiento.Finalizado)
            throw new InvalidOperationException("No se puede cancelar una orden finalizada");
        
        Estado = EstadoMantenimiento.Cancelado;
        Observaciones = $"CANCELADO: {motivo}";
    }
    
    public void AsignarProveedor(string proveedor)
    {
        Proveedor = proveedor;
    }
    
    public void EstablecerProximoMantenimiento(decimal kmProximo)
    {
        KmProximoMantenimiento = kmProximo;
    }
    
    public bool EstaVencida => Estado == EstadoMantenimiento.Pendiente && FechaProgramada < DateTime.UtcNow;
    
    public AlertStatus GetAlerta()
    {
        if (Estado == EstadoMantenimiento.Finalizado || Estado == EstadoMantenimiento.Cancelado)
            return AlertStatus.Verde;
        
        var diasRestantes = (FechaProgramada - DateTime.UtcNow).Days;
        return AlertStatus.FromDaysToExpiry(diasRestantes);
    }
    
    public decimal VariacionCosto => CostoReal > 0 ? CostoReal - CostoEstimado : 0;
}

/// <summary>
/// Carga de Combustible - Entity
/// </summary>
public class FuelLoad : Entity
{
    public int VehicleId { get; private set; }
    public Vehicle? Vehicle { get; private set; }
    
    public DateTime Fecha { get; private set; }
    public decimal Galones { get; private set; }
    public decimal PrecioPorGalon { get; private set; }
    public decimal Total => Galones * PrecioPorGalon;
    
    public decimal KmOdometro { get; private set; }
    public string? Estacion { get; private set; }
    public string? NumeroVoucher { get; private set; }
    
    public int? ConductorId { get; private set; }
    public Driver? Conductor { get; private set; }
    
    private FuelLoad() { }
    
    public static FuelLoad Create(
        int vehicleId,
        decimal galones,
        decimal precioPorGalon,
        decimal kmOdometro,
        string? estacion = null,
        int? conductorId = null)
    {
        if (galones <= 0)
            throw new ArgumentException("Los galones deben ser mayores a cero");
        
        if (precioPorGalon <= 0)
            throw new ArgumentException("El precio debe ser mayor a cero");
        
        return new FuelLoad
        {
            VehicleId = vehicleId,
            Fecha = DateTime.UtcNow,
            Galones = galones,
            PrecioPorGalon = precioPorGalon,
            KmOdometro = kmOdometro,
            Estacion = estacion,
            ConductorId = conductorId
        };
    }
    
    public void AsignarVoucher(string voucher)
    {
        NumeroVoucher = voucher;
    }
}

/// <summary>
/// Documento Vehicular - Entity
/// </summary>
public class VehicleDocument : Entity
{
    public int VehicleId { get; private set; }
    public Vehicle? Vehicle { get; private set; }
    
    public TipoDocumento Tipo { get;  private set; }
    public string NumeroDocumento { get; private set; } = string.Empty;
    
    public DateTime FechaEmision { get; private set; }
    public DateTime FechaVencimiento { get; private set; }
    
    public string? ArchivoUrl { get; private set; }
    public string? Emisor { get; private set; }
    public decimal? Costo { get; private set; }
    
    public bool Activo { get; private set; } = true;
    
    private VehicleDocument() { }
    
    public static VehicleDocument Create(
        int vehicleId,
        TipoDocumento tipo,
        string numeroDocumento,
        DateTime fechaEmision,
        DateTime fechaVencimiento,
        string? emisor = null,
        decimal? costo = null)
    {
        if (fechaVencimiento <= fechaEmision)
            throw new ArgumentException("La fecha de vencimiento debe ser posterior a la emisión");
        
        return new VehicleDocument
        {
            VehicleId = vehicleId,
            Tipo = tipo,
            NumeroDocumento = numeroDocumento,
            FechaEmision = fechaEmision,
            FechaVencimiento = fechaVencimiento,
            Emisor = emisor,
            Costo = costo
        };
    }
    
    public void AdjuntarArchivo(string url)
    {
        ArchivoUrl = url;
    }
    
    public void Renovar(DateTime nuevaFechaVencimiento, string? nuevoNumero = null)
    {
        if (nuevaFechaVencimiento <= DateTime.UtcNow)
            throw new ArgumentException("La fecha de vencimiento debe ser futura");
        
        FechaVencimiento = nuevaFechaVencimiento;
        if (nuevoNumero != null)
            NumeroDocumento = nuevoNumero;
    }
    
    public void Desactivar()
    {
        Activo = false;
    }
    
    public bool EstaVigente => Activo && FechaVencimiento > DateTime.UtcNow;
    
    public bool EstaVencido => FechaVencimiento <= DateTime.UtcNow;
    
    public int DiasParaVencimiento => Math.Max(0, (FechaVencimiento - DateTime.UtcNow).Days);
    
    public AlertStatus GetAlerta() => AlertStatus.FromDaysToExpiry(DiasParaVencimiento);
}
