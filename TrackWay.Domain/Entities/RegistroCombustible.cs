namespace TrackWay.Domain.Entities;

/// <summary>
/// Representa un registro de carga de combustible
/// </summary>
public class RegistroCombustible
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public decimal CantidadLitros { get; set; }
    public decimal PrecioPorLitro { get; set; }
    public decimal Total { get; set; }
    public int Kilometraje { get; set; }
    public string Estacion { get; set; } = string.Empty;
    public string TipoCombustible { get; set; } = string.Empty;
    public string NumeroFactura { get; set; } = string.Empty;
    public string? Observaciones { get; set; }

    // Navegación
    public int VehiculoId { get; set; }
    public Vehiculo Vehiculo { get; set; } = null!;
}
