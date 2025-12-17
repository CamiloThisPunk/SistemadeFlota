using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackWay.Domain.Entities.Fleet;
using TrackWay.Domain.Enums;
using TrackWay.Domain.DTOs;
using TrackWay.Infrastructure.Persistence;

namespace TrackWay.API.Controllers.Fleet;

[ApiController]
[Route("api/fleet/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly TrackWayDbContext _context;

    public DocumentsController(TrackWayDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleDocumentDto>>> GetAll()
    {
        var docs = await _context.VehicleDocuments
            .Include(d => d.Vehicle)
            .Where(d => d.Activo)
            .OrderBy(d => d.FechaVencimiento)
            .ToListAsync();

        var dtos = docs.Select(d => new VehicleDocumentDto(
            d.Id,
            d.VehicleId,
            d.Tipo.ToString(),
            d.NumeroDocumento,
            d.FechaEmision,
            d.FechaVencimiento,
            d.DiasParaVencimiento,
            d.EstaVigente,
            d.ArchivoUrl,
            d.GetAlerta().Color));

        return Ok(dtos);
    }

    [HttpGet("por-vencer")]
    public async Task<ActionResult<IEnumerable<VehicleDocumentDto>>> GetPorVencer()
    {
        var fechaLimite = DateTime.UtcNow.AddDays(30);
        var docs = await _context.VehicleDocuments
            .Include(d => d.Vehicle)
            .Where(d => d.Activo && d.FechaVencimiento <= fechaLimite)
            .OrderBy(d => d.FechaVencimiento)
            .ToListAsync();

        var dtos = docs.Select(d => new VehicleDocumentDto(
            d.Id,
            d.VehicleId,
            d.Tipo.ToString(),
            d.NumeroDocumento,
            d.FechaEmision,
            d.FechaVencimiento,
            d.DiasParaVencimiento,
            d.EstaVigente,
            d.ArchivoUrl,
            d.GetAlerta().Color));

        return Ok(dtos);
    }

    [HttpGet("vehiculo/{vehicleId}")]
    public async Task<ActionResult<IEnumerable<VehicleDocumentDto>>> GetByVehicle(int vehicleId)
    {
        var docs = await _context.VehicleDocuments
            .Include(d => d.Vehicle)
            .Where(d => d.VehicleId == vehicleId && d.Activo)
            .OrderBy(d => d.FechaVencimiento)
            .ToListAsync();

        var dtos = docs.Select(d => new VehicleDocumentDto(
            d.Id,
            d.VehicleId,
            d.Tipo.ToString(),
            d.NumeroDocumento,
            d.FechaEmision,
            d.FechaVencimiento,
            d.DiasParaVencimiento,
            d.EstaVigente,
            d.ArchivoUrl,
            d.GetAlerta().Color));

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleDocumentDto>> Create([FromBody] DocumentCreateDto dto)
    {
        var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId);
        if (vehicle == null)
            return NotFound("Vehículo no encontrado");

        if (!Enum.TryParse<TipoDocumento>(dto.Tipo, true, out var tipo))
            return BadRequest("Tipo de documento no válido");

        var doc = VehicleDocument.Create(
            dto.VehicleId,
            tipo,
            dto.NumeroDocumento,
            dto.FechaEmision,
            dto.FechaVencimiento,
            dto.Emisor,
            dto.Costo);

        _context.VehicleDocuments.Add(doc);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByVehicle), new { vehicleId = dto.VehicleId },
            new VehicleDocumentDto(doc.Id, doc.VehicleId, doc.Tipo.ToString(), doc.NumeroDocumento,
                doc.FechaEmision, doc.FechaVencimiento, doc.DiasParaVencimiento, doc.EstaVigente,
                doc.ArchivoUrl, doc.GetAlerta().Color));
    }

    [HttpPut("{id}/renovar")]
    public async Task<IActionResult> Renovar(int id, [FromBody] RenovarDocumentoRequest request)
    {
        var doc = await _context.VehicleDocuments.FindAsync(id);
        if (doc == null)
            return NotFound();

        try
        {
            doc.Renovar(request.NuevaFechaVencimiento, request.NuevoNumero);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/adjuntar")]
    public async Task<IActionResult> AdjuntarArchivo(int id, [FromBody] string archivoUrl)
    {
        var doc = await _context.VehicleDocuments.FindAsync(id);
        if (doc == null)
            return NotFound();

        doc.AdjuntarArchivo(archivoUrl);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _context.VehicleDocuments.FindAsync(id);
        if (doc == null)
            return NotFound();

        doc.Desactivar();
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public record RenovarDocumentoRequest(DateTime NuevaFechaVencimiento, string? NuevoNumero);
