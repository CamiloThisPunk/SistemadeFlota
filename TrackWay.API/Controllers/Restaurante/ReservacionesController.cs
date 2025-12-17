using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackWay.Domain.Entities.Restaurante;
using TrackWay.Infrastructure.Persistence;

namespace TrackWay.API.Controllers.Restaurante;

[ApiController]
[Route("api/restaurante/[controller]")]
[Tags("Restaurante - Reservaciones")]
public class ReservacionesController : ControllerBase
{
    private readonly TrackWayDbContext _context;

    public ReservacionesController(TrackWayDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reservacion>>> GetAll()
    {
        return await _context.Reservaciones
            .Include(r => r.Mesa)
            .OrderByDescending(r => r.FechaHora)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Reservacion>> GetById(int id)
    {
        var reservacion = await _context.Reservaciones
            .Include(r => r.Mesa)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservacion == null)
            return NotFound(new { message = $"Reservación con ID {id} no encontrada" });

        return reservacion;
    }

    [HttpPost]
    public async Task<ActionResult<Reservacion>> Create(Reservacion reservacion)
    {
        // Validar que la mesa existe
        var mesa = await _context.Mesas.FindAsync(reservacion.MesaId);
        if (mesa == null)
            return BadRequest(new { message = "Mesa no encontrada" });

        // Verificar disponibilidad de la mesa en esa fecha/hora
        var conflicto = await _context.Reservaciones.AnyAsync(r =>
            r.MesaId == reservacion.MesaId &&
            r.FechaHora.Date == reservacion.FechaHora.Date &&
            Math.Abs((r.FechaHora - reservacion.FechaHora).TotalHours) < 2 &&
            r.Estado != "Cancelada" && r.Estado != "Completada" && r.Estado != "NoShow");

        if (conflicto)
            return BadRequest(new { message = "La mesa ya tiene una reservación en ese horario" });

        // Generar código
        reservacion.Codigo = $"RES-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        reservacion.FechaCreacion = DateTime.UtcNow;

        _context.Reservaciones.Add(reservacion);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = reservacion.Id }, reservacion);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Reservacion reservacion)
    {
        if (id != reservacion.Id)
            return BadRequest(new { message = "ID no coincide" });

        var existing = await _context.Reservaciones.FindAsync(id);
        if (existing == null)
            return NotFound(new { message = $"Reservación con ID {id} no encontrada" });

        existing.NombreCliente = reservacion.NombreCliente;
        existing.Telefono = reservacion.Telefono;
        existing.Email = reservacion.Email;
        existing.FechaHora = reservacion.FechaHora;
        existing.NumeroPersonas = reservacion.NumeroPersonas;
        existing.Observaciones = reservacion.Observaciones;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/confirmar")]
    public async Task<IActionResult> Confirmar(int id)
    {
        var reservacion = await _context.Reservaciones.Include(r => r.Mesa).FirstOrDefaultAsync(r => r.Id == id);
        if (reservacion == null)
            return NotFound(new { message = $"Reservación con ID {id} no encontrada" });

        reservacion.Estado = "Confirmada";
        await _context.SaveChangesAsync();
        return Ok(new { message = "Reservación confirmada" });
    }

    [HttpPatch("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var reservacion = await _context.Reservaciones.Include(r => r.Mesa).FirstOrDefaultAsync(r => r.Id == id);
        if (reservacion == null)
            return NotFound(new { message = $"Reservación con ID {id} no encontrada" });

        reservacion.Estado = "Cancelada";
        if (reservacion.Mesa != null && reservacion.Mesa.Estado == "Reservada")
            reservacion.Mesa.Estado = "Disponible";

        await _context.SaveChangesAsync();
        return Ok(new { message = "Reservación cancelada" });
    }

    [HttpPatch("{id}/completar")]
    public async Task<IActionResult> Completar(int id)
    {
        var reservacion = await _context.Reservaciones.Include(r => r.Mesa).FirstOrDefaultAsync(r => r.Id == id);
        if (reservacion == null)
            return NotFound(new { message = $"Reservación con ID {id} no encontrada" });

        reservacion.Estado = "Completada";
        if (reservacion.Mesa != null)
            reservacion.Mesa.Estado = "Ocupada";

        await _context.SaveChangesAsync();
        return Ok(new { message = "Cliente llegó, mesa ocupada" });
    }

    [HttpGet("fecha/{fecha}")]
    public async Task<ActionResult<IEnumerable<Reservacion>>> GetByFecha(DateTime fecha)
    {
        return await _context.Reservaciones
            .Include(r => r.Mesa)
            .Where(r => r.FechaHora.Date == fecha.Date)
            .OrderBy(r => r.FechaHora)
            .ToListAsync();
    }

    [HttpGet("hoy")]
    public async Task<ActionResult<IEnumerable<Reservacion>>> GetHoy()
    {
        var hoy = DateTime.UtcNow.Date;
        return await _context.Reservaciones
            .Include(r => r.Mesa)
            .Where(r => r.FechaHora.Date == hoy)
            .OrderBy(r => r.FechaHora)
            .ToListAsync();
    }
}
