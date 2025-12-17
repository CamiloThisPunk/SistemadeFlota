namespace TrackWay.Domain.Entities;

/// <summary>
/// Representa un vehículo de la flota
/// </summary>
public class Vehiculo
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Año { get; set; }
    public string Color { get; set; } = string.Empty;
    public string TipoCombustible { get; set; } = string.Empty;
    public decimal CapacidadTanque { get; set; }
    public int Kilometraje { get; set; }
    public string Estado { get; set; } = "Disponible"; // Disponible, En Ruta, Mantenimiento, Fuera de Servicio
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;

    // Navegación
    public int? ConductorActualId { get; set; }
    public Conductor? ConductorActual { get; set; }
    public ICollection<Ruta> Rutas { get; set; } = new List<Ruta>();
    public ICollection<Mantenimiento> Mantenimientos { get; set; } = new List<Mantenimiento>();
    public ICollection<RegistroCombustible> RegistrosCombustible { get; set; } = new List<RegistroCombustible>();
}
