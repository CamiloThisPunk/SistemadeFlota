namespace TrackWay.Domain.Entities;

/// <summary>
/// Representa un registro de mantenimiento de un vehículo
/// </summary>
public class Mantenimiento
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty; // Preventivo, Correctivo, Emergencia
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaProgramada { get; set; }
    public DateTime? FechaRealizacion { get; set; }
    public int KilometrajeMantenimiento { get; set; }
    public decimal Costo { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public string Estado { get; set; } = "Programado"; // Programado, En Proceso, Completado, Cancelado
    public string? Observaciones { get; set; }

    // Navegación
    public int VehiculoId { get; set; }
    public Vehiculo Vehiculo { get; set; } = null!;
}
