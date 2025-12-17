namespace TrackWay.Domain.Entities.Restaurante;

/// <summary>
/// Orden o pedido del restaurante
/// </summary>
public class Orden
{
    public int Id { get; set; }
    public string NumeroOrden { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    public string TipoServicio { get; set; } = "Mesa"; // Mesa, Llevar, Delivery
    public string Estado { get; set; } = "Pendiente"; // Pendiente, EnPreparacion, Listo, Entregado, Cancelado, Pagado
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
    public string? NombreCliente { get; set; }
    public string? TelefonoCliente { get; set; }
    public string? DireccionEntrega { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaHoraPago { get; set; }
    public string? MetodoPago { get; set; } // Efectivo, Tarjeta, Transferencia

    // Navegación
    public int? MesaId { get; set; }
    public Mesa? Mesa { get; set; }
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    public ICollection<DetalleOrden> Detalles { get; set; } = new List<DetalleOrden>();
}
