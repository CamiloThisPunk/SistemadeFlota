namespace TrackWay.Domain.Entities;

/// <summary>
/// Representa un conductor de la flota
/// </summary>
public class Conductor
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = "DNI";
    public string NumeroLicencia { get; set; } = string.Empty;
    public string CategoriaLicencia { get; set; } = string.Empty;
    public DateTime FechaVencimientoLicencia { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public DateTime FechaContratacion { get; set; }
    public string Estado { get; set; } = "Disponible"; // Disponible, En Ruta, Descanso, Inactivo
    public bool Activo { get; set; } = true;

    // Navegación
    public int? VehiculoAsignadoId { get; set; }
    public Vehiculo? VehiculoAsignado { get; set; }
    public ICollection<Ruta> Rutas { get; set; } = new List<Ruta>();
}
