using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackWay.Domain.Entities.Restaurante;
using TrackWay.Infrastructure.Persistence;

namespace TrackWay.API.Controllers.Restaurante;

[ApiController]
[Route("api/restaurante/[controller]")]
[Tags("Restaurante - Productos")]
public class ProductosController : ControllerBase
{
    private readonly TrackWayDbContext _context;

    public ProductosController(TrackWayDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> GetAll()
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> GetById(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null)
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });

        return producto;
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> Create(Producto producto)
    {
        if (await _context.Productos.AnyAsync(p => p.Codigo == producto.Codigo))
            return BadRequest(new { message = $"Ya existe un producto con el código {producto.Codigo}" });

        var categoria = await _context.Categorias.FindAsync(producto.CategoriaId);
        if (categoria == null)
            return BadRequest(new { message = "Categoría no encontrada" });

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Producto producto)
    {
        if (id != producto.Id)
            return BadRequest(new { message = "ID no coincide" });

        var existing = await _context.Productos.FindAsync(id);
        if (existing == null)
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });

        existing.Nombre = producto.Nombre;
        existing.Descripcion = producto.Descripcion;
        existing.Precio = producto.Precio;
        existing.PrecioDescuento = producto.PrecioDescuento;
        existing.ImagenUrl = producto.ImagenUrl;
        existing.TiempoPreparacionMinutos = producto.TiempoPreparacionMinutos;
        existing.Disponible = producto.Disponible;
        existing.CategoriaId = producto.CategoriaId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });

        producto.Activo = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("categoria/{categoriaId}")]
    public async Task<ActionResult<IEnumerable<Producto>>> GetByCategoria(int categoriaId)
    {
        return await _context.Productos
            .Where(p => p.CategoriaId == categoriaId && p.Activo && p.Disponible)
            .ToListAsync();
    }

    [HttpGet("disponibles")]
    public async Task<ActionResult<IEnumerable<Producto>>> GetDisponibles()
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.Disponible)
            .ToListAsync();
    }

    [HttpPatch("{id}/disponibilidad")]
    public async Task<IActionResult> ToggleDisponibilidad(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });

        producto.Disponible = !producto.Disponible;
        await _context.SaveChangesAsync();
        return Ok(new { disponible = producto.Disponible });
    }
}
