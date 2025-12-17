namespace TrackWay.Domain.Entities;

/// <summary>
/// Representa una ruta o viaje realizado por un vehículo
/// </summary>
public class Ruta
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public decimal DistanciaKm { get; set; }
    public DateTime FechaHoraSalida { get; set; }
    public DateTime? FechaHoraLlegada { get; set; }
    public int KilometrajeInicial { get; set; }
    public int? KilometrajeFinal { get; set; }
    public string Estado { get; set; } = "Programada"; // Programada, En Curso, Completada, Cancelada
    public string? Observaciones { get; set; }

    // Navegación
    public int VehiculoId { get; set; }
    public Vehiculo Vehiculo { get; set; } = null!;
    public int ConductorId { get; set; }
    public Conductor Conductor { get; set; } = null!;
}
