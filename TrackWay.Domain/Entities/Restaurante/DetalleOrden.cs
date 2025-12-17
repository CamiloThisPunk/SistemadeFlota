namespace TrackWay.Domain.Entities.Restaurante;

/// <summary>
/// Detalle de cada producto en una orden
/// </summary>
public class DetalleOrden
{
    public int Id { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente, EnPreparacion, Listo, Entregado
    public string? Notas { get; set; } // Notas especiales (sin sal, extra picante, etc.)

    // Navegación
    public int OrdenId { get; set; }
    public Orden Orden { get; set; } = null!;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
}
