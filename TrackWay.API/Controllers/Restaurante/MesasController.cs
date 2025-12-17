using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackWay.Domain.Entities.Restaurante;
using TrackWay.Infrastructure.Persistence;

namespace TrackWay.API.Controllers.Restaurante;

[ApiController]
[Route("api/restaurante/[controller]")]
[Tags("Restaurante - Mesas")]
public class MesasController : ControllerBase
{
    private readonly TrackWayDbContext _context;

    public MesasController(TrackWayDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Mesa>>> GetAll()
    {
        return await _context.Mesas
            .Where(m => m.Activo)
            .OrderBy(m => m.Numero)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Mesa>> GetById(int id)
    {
        var mesa = await _context.Mesas
            .Include(m => m.Ordenes.Where(o => o.Estado != "Pagado"))
            .FirstOrDefaultAsync(m => m.Id == id);

        if (mesa == null)
            return NotFound(new { message = $"Mesa con ID {id} no encontrada" });

        return mesa;
    }

    [HttpPost]
    public async Task<ActionResult<Mesa>> Create(Mesa mesa)
    {
        if (await _context.Mesas.AnyAsync(m => m.Numero == mesa.Numero))
            return BadRequest(new { message = $"Ya existe una mesa con el número {mesa.Numero}" });

        _context.Mesas.Add(mesa);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = mesa.Id }, mesa);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Mesa mesa)
    {
        if (id != mesa.Id)
            return BadRequest(new { message = "ID no coincide" });

        var existing = await _context.Mesas.FindAsync(id);
        if (existing == null)
            return NotFound(new { message = $"Mesa con ID {id} no encontrada" });

        existing.Numero = mesa.Numero;
        existing.Capacidad = mesa.Capacidad;
        existing.Ubicacion = mesa.Ubicacion;
        existing.Estado = mesa.Estado;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var mesa = await _context.Mesas.FindAsync(id);
        if (mesa == null)
            return NotFound(new { message = $"Mesa con ID {id} no encontrada" });

        mesa.Activo = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("disponibles")]
    public async Task<ActionResult<IEnumerable<Mesa>>> GetDisponibles()
    {
        return await _context.Mesas
            .Where(m => m.Estado == "Disponible" && m.Activo)
            .OrderBy(m => m.Numero)
            .ToListAsync();
    }

    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] string nuevoEstado)
    {
        var mesa = await _context.Mesas.FindAsync(id);
        if (mesa == null)
            return NotFound(new { message = $"Mesa con ID {id} no encontrada" });

        mesa.Estado = nuevoEstado;
        await _context.SaveChangesAsync();
        return Ok(new { estado = mesa.Estado });
    }

    [HttpGet("mapa")]
    public async Task<ActionResult<object>> GetMapaMesas()
    {
        var mesas = await _context.Mesas.Where(m => m.Activo).ToListAsync();
        return new
        {
            Total = mesas.Count,
            Disponibles = mesas.Count(m => m.Estado == "Disponible"),
            Ocupadas = mesas.Count(m => m.Estado == "Ocupada"),
            Reservadas = mesas.Count(m => m.Estado == "Reservada"),
            Mesas = mesas
        };
    }
}
