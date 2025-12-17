namespace TrackWay.Domain.Entities.Restaurante;

/// <summary>
/// Producto o platillo del menú
/// </summary>
public class Producto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public decimal? PrecioDescuento { get; set; }
    public string? ImagenUrl { get; set; }
    public int TiempoPreparacionMinutos { get; set; }
    public bool Disponible { get; set; } = true;
    public bool Activo { get; set; } = true;

    // Navegación
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
    public ICollection<DetalleOrden> DetallesOrden { get; set; } = new List<DetalleOrden>();
}
