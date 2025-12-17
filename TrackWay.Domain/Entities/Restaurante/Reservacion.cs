namespace TrackWay.Domain.Entities.Restaurante;

/// <summary>
/// Reservación de mesa
/// </summary>
public class Reservacion
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime FechaHora { get; set; }
    public int NumeroPersonas { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Confirmada, Cancelada, Completada, NoShow
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Navegación
    public int MesaId { get; set; }
    public Mesa Mesa { get; set; } = null!;
}
