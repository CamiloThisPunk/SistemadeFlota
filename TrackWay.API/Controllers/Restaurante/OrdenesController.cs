using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackWay.Domain.Entities.Restaurante;
using TrackWay.Infrastructure.Persistence;

namespace TrackWay.API.Controllers.Restaurante;

[ApiController]
[Route("api/restaurante/[controller]")]
[Tags("Restaurante - Órdenes")]
public class OrdenesController : ControllerBase
{
    private readonly TrackWayDbContext _context;

    public OrdenesController(TrackWayDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Orden>>> GetAll()
    {
        return await _context.Ordenes
            .Include(o => o.Mesa)
            .Include(o => o.Empleado)
            .Include(o => o.Detalles)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(o => o.FechaHora)
            .Take(100)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Orden>> GetById(int id)
    {
        var orden = await _context.Ordenes
            .Include(o => o.Mesa)
            .Include(o => o.Empleado)
            .Include(o => o.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null)
            return NotFound(new { message = $"Orden con ID {id} no encontrada" });

        return orden;
    }

    [HttpPost]
    public async Task<ActionResult<Orden>> Create(Orden orden)
    {
        // Validar empleado
        var empleado = await _context.Empleados.FindAsync(orden.EmpleadoId);
        if (empleado == null)
            return BadRequest(new { message = "Empleado no encontrado" });

        // Validar mesa si aplica
        if (orden.MesaId.HasValue)
        {
            var mesa = await _context.Mesas.FindAsync(orden.MesaId);
            if (mesa == null)
                return BadRequest(new { message = "Mesa no encontrada" });

            mesa.Estado = "Ocupada";
        }

        // Generar número de orden
        orden.NumeroOrden = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}";
        orden.FechaHora = DateTime.UtcNow;

        // Calcular totales
        orden.Subtotal = orden.Detalles.Sum(d => d.Subtotal);
        orden.Total = orden.Subtotal + orden.Impuestos - orden.Descuento;

        _context.Ordenes.Add(orden);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = orden.Id }, orden);
    }

    [HttpPost("{id}/agregar-producto")]
    public async Task<IActionResult> AgregarProducto(int id, [FromBody] DetalleOrden detalle)
    {
        var orden = await _context.Ordenes.Include(o => o.Detalles).FirstOrDefaultAsync(o => o.Id == id);
        if (orden == null)
            return NotFound(new { message = $"Orden con ID {id} no encontrada" });

        var producto = await _context.Productos.FindAsync(detalle.ProductoId);
        if (producto == null)
            return BadRequest(new { message = "Producto no encontrado" });

        detalle.PrecioUnitario = producto.PrecioDescuento ?? producto.Precio;
        detalle.Subtotal = detalle.PrecioUnitario * detalle.Cantidad;
        detalle.OrdenId = id;

        orden.Detalles.Add(detalle);
        orden.Subtotal = orden.Detalles.Sum(d => d.Subtotal);
        orden.Total = orden.Subtotal + orden.Impuestos - orden.Descuento;

        await _context.SaveChangesAsync();
        return Ok(orden);
    }

    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] string nuevoEstado)
    {
        var orden = await _context.Ordenes.Include(o => o.Mesa).FirstOrDefaultAsync(o => o.Id == id);
        if (orden == null)
            return NotFound(new { message = $"Orden con ID {id} no encontrada" });

        orden.Estado = nuevoEstado;

        // Si se paga, liberar mesa
        if (nuevoEstado == "Pagado" && orden.Mesa != null)
        {
            orden.FechaHoraPago = DateTime.UtcNow;
            orden.Mesa.Estado = "Disponible";
        }

        await _context.SaveChangesAsync();
        return Ok(new { estado = orden.Estado });
    }

    [HttpPatch("{id}/pagar")]
    public async Task<IActionResult> PagarOrden(int id, [FromBody] string metodoPago)
    {
        var orden = await _context.Ordenes.Include(o => o.Mesa).FirstOrDefaultAsync(o => o.Id == id);
        if (orden == null)
            return NotFound(new { message = $"Orden con ID {id} no encontrada" });

        orden.Estado = "Pagado";
        orden.MetodoPago = metodoPago;
        orden.FechaHoraPago = DateTime.UtcNow;

        if (orden.Mesa != null)
            orden.Mesa.Estado = "Disponible";

        await _context.SaveChangesAsync();
        return Ok(new { message = "Orden pagada", total = orden.Total });
    }

    [HttpGet("activas")]
    public async Task<ActionResult<IEnumerable<Orden>>> GetActivas()
    {
        return await _context.Ordenes
            .Include(o => o.Mesa)
            .Include(o => o.Empleado)
            .Include(o => o.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(o => o.Estado != "Pagado" && o.Estado != "Cancelado")
            .OrderBy(o => o.FechaHora)
            .ToListAsync();
    }

    [HttpGet("mesa/{mesaId}")]
    public async Task<ActionResult<Orden?>> GetOrdenActualMesa(int mesaId)
    {
        return await _context.Ordenes
            .Include(o => o.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(o => o.MesaId == mesaId && o.Estado != "Pagado" && o.Estado != "Cancelado")
            .OrderByDescending(o => o.FechaHora)
            .FirstOrDefaultAsync();
    }
}
