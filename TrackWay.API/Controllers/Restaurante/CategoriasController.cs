using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackWay.Domain.Entities.Restaurante;
using TrackWay.Infrastructure.Persistence;

namespace TrackWay.API.Controllers.Restaurante;

[ApiController]
[Route("api/restaurante/[controller]")]
[Tags("Restaurante - Categorías")]
public class CategoriasController : ControllerBase
{
    private readonly TrackWayDbContext _context;

    public CategoriasController(TrackWayDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Categoria>>> GetAll()
    {
        return await _context.Categorias
            .Where(c => c.Activo)
            .OrderBy(c => c.Orden)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Categoria>> GetById(int id)
    {
        var categoria = await _context.Categorias
            .Include(c => c.Productos.Where(p => p.Activo))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
            return NotFound(new { message = $"Categoría con ID {id} no encontrada" });

        return categoria;
    }

    [HttpPost]
    public async Task<ActionResult<Categoria>> Create(Categoria categoria)
    {
        if (await _context.Categorias.AnyAsync(c => c.Nombre == categoria.Nombre))
            return BadRequest(new { message = $"Ya existe una categoría con el nombre {categoria.Nombre}" });

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Categoria categoria)
    {
        if (id != categoria.Id)
            return BadRequest(new { message = "ID no coincide" });

        var existing = await _context.Categorias.FindAsync(id);
        if (existing == null)
            return NotFound(new { message = $"Categoría con ID {id} no encontrada" });

        existing.Nombre = categoria.Nombre;
        existing.Descripcion = categoria.Descripcion;
        existing.Icono = categoria.Icono;
        existing.Orden = categoria.Orden;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
            return NotFound(new { message = $"Categoría con ID {id} no encontrada" });

        categoria.Activo = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
