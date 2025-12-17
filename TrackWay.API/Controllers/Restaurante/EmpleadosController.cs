using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackWay.Domain.Entities.Restaurante;
using TrackWay.Infrastructure.Persistence;

namespace TrackWay.API.Controllers.Restaurante;

[ApiController]
[Route("api/restaurante/[controller]")]
[Tags("Restaurante - Empleados")]
public class EmpleadosController : ControllerBase
{
    private readonly TrackWayDbContext _context;

    public EmpleadosController(TrackWayDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Empleado>>> GetAll()
    {
        return await _context.Empleados
            .Where(e => e.Activo)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Empleado>> GetById(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);

        if (empleado == null)
            return NotFound(new { message = $"Empleado con ID {id} no encontrado" });

        return empleado;
    }

    [HttpPost]
    public async Task<ActionResult<Empleado>> Create(Empleado empleado)
    {
        if (await _context.Empleados.AnyAsync(e => e.Documento == empleado.Documento))
            return BadRequest(new { message = $"Ya existe un empleado con el documento {empleado.Documento}" });

        empleado.Codigo = $"EMP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        _context.Empleados.Add(empleado);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = empleado.Id }, empleado);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Empleado empleado)
    {
        if (id != empleado.Id)
            return BadRequest(new { message = "ID no coincide" });

        var existing = await _context.Empleados.FindAsync(id);
        if (existing == null)
            return NotFound(new { message = $"Empleado con ID {id} no encontrado" });

        existing.Nombres = empleado.Nombres;
        existing.Apellidos = empleado.Apellidos;
        existing.Cargo = empleado.Cargo;
        existing.Telefono = empleado.Telefono;
        existing.Email = empleado.Email;
        existing.SalarioBase = empleado.SalarioBase;
        existing.Estado = empleado.Estado;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);
        if (empleado == null)
            return NotFound(new { message = $"Empleado con ID {id} no encontrado" });

        empleado.Activo = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("cargo/{cargo}")]
    public async Task<ActionResult<IEnumerable<Empleado>>> GetByCargo(string cargo)
    {
        return await _context.Empleados
            .Where(e => e.Cargo == cargo && e.Activo && e.Estado == "Activo")
            .ToListAsync();
    }

    [HttpGet("meseros")]
    public async Task<ActionResult<IEnumerable<Empleado>>> GetMeseros()
    {
        return await _context.Empleados
            .Where(e => e.Cargo == "Mesero" && e.Activo && e.Estado == "Activo")
            .ToListAsync();
    }
}
