namespace TrackWay.Domain.Entities.Restaurante;

/// <summary>
/// Categoría de productos del menú
/// </summary>
public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;

    // Navegación
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
