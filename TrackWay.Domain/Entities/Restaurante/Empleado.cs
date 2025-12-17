namespace TrackWay.Domain.Entities.Restaurante;

/// <summary>
/// Empleado del restaurante (mesero, cocinero, cajero, etc.)
/// </summary>
public class Empleado
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty; // Mesero, Cocinero, Cajero, Gerente
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
    public decimal SalarioBase { get; set; }
    public DateTime FechaContratacion { get; set; }
    public string Estado { get; set; } = "Activo"; // Activo, Inactivo, Vacaciones
    public bool Activo { get; set; } = true;

    // Navegación
    public ICollection<Orden> OrdenesAtendidas { get; set; } = new List<Orden>();
}
