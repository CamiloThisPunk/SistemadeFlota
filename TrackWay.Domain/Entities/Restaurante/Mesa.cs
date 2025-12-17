namespace TrackWay.Domain.Entities.Restaurante;

/// <summary>
/// Mesa del restaurante
/// </summary>
public class Mesa
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public string Ubicacion { get; set; } = string.Empty; // Interior, Terraza, VIP, etc.
    public string Estado { get; set; } = "Disponible"; // Disponible, Ocupada, Reservada, Mantenimiento
    public bool Activo { get; set; } = true;

    // Navegación
    public ICollection<Orden> Ordenes { get; set; } = new List<Orden>();
    public ICollection<Reservacion> Reservaciones { get; set; } = new List<Reservacion>();
}
