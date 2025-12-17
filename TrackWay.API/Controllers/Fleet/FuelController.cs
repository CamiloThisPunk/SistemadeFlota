using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackWay.Domain.Entities.Fleet;
using TrackWay.Domain.Enums;
using TrackWay.Domain.DTOs;
using TrackWay.Infrastructure.Persistence;

namespace TrackWay.API.Controllers.Fleet;

[ApiController]
[Route("api/fleet/[controller]")]
public class FuelController : ControllerBase
{
    private readonly TrackWayDbContext _context;

    public FuelController(TrackWayDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FuelLoadDto>>> GetAll()
    {
        var loads = await _context.FuelLoads
            .Include(f => f.Vehicle)
            .Include(f => f.Conductor)
            .OrderByDescending(f => f.Fecha)
            .Take(100)
            .ToListAsync();

        var dtos = loads.Select(f => new FuelLoadDto(
            f.Id,
            f.VehicleId,
            f.Vehicle?.Placa ?? "",
            f.Fecha,
            f.Galones,
            f.PrecioPorGalon,
            f.Total,
            f.KmOdometro,
            f.Estacion,
            f.Conductor != null ? $"{f.Conductor.Nombre} {f.Conductor.Apellidos}" : null));

        return Ok(dtos);
    }

    [HttpGet("vehiculo/{vehicleId}")]
    public async Task<ActionResult<IEnumerable<FuelLoadDto>>> GetByVehicle(int vehicleId)
    {
        var loads = await _context.FuelLoads
            .Include(f => f.Vehicle)
            .Include(f => f.Conductor)
            .Where(f => f.VehicleId == vehicleId)
            .OrderByDescending(f => f.Fecha)
            .ToListAsync();

        var dtos = loads.Select(f => new FuelLoadDto(
            f.Id,
            f.VehicleId,
            f.Vehicle?.Placa ?? "",
            f.Fecha,
            f.Galones,
            f.PrecioPorGalon,
            f.Total,
            f.KmOdometro,
            f.Estacion,
            f.Conductor != null ? $"{f.Conductor.Nombre} {f.Conductor.Apellidos}" : null));

        return Ok(dtos);
    }

    [HttpGet("vehiculo/{vehicleId}/resumen")]
    public async Task<ActionResult<object>> GetResumenVehiculo(int vehicleId)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.CargasCombustible)
            .FirstOrDefaultAsync(v => v.Id == vehicleId);

        if (vehicle == null)
            return NotFound();

        var mesActual = DateTime.UtcNow.Month;
        var añoActual = DateTime.UtcNow.Year;

        var cargasMes = vehicle.CargasCombustible
            .Where(c => c.Fecha.Month == mesActual && c.Fecha.Year == añoActual)
            .ToList();

        return Ok(new
        {
            TotalCargasHistoricas = vehicle.CargasCombustible.Count,
            GalonesTotales = vehicle.CargasCombustible.Sum(c => c.Galones),
            GastoTotal = vehicle.CargasCombustible.Sum(c => c.Total),
            CargasMes = cargasMes.Count,
            GalonesMes = cargasMes.Sum(c => c.Galones),
            GastoMes = cargasMes.Sum(c => c.Total),
            ConsumoPorcentaje = vehicle.CalcularConsumoPromedio()
        });
    }

    [HttpPost]
    public async Task<ActionResult<FuelLoadDto>> Create([FromBody] FuelLoadCreateDto dto)
    {
        var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId);
        if (vehicle == null)
            return NotFound("Vehículo no encontrado");

        if (dto.KmOdometro < vehicle.KmActual)
            return BadRequest("El kilometraje no puede ser menor al actual del vehículo");

        Driver? conductor = null;
        if (dto.ConductorId.HasValue)
        {
            conductor = await _context.Drivers.FindAsync(dto.ConductorId);
            if (conductor == null)
                return NotFound("Conductor no encontrado");
        }

        var fuelLoad = FuelLoad.Create(
            dto.VehicleId,
            dto.Galones,
            dto.PrecioPorGalon,
            dto.KmOdometro,
            dto.Estacion,
            dto.ConductorId);

        // Actualizar kilometraje del vehículo
        vehicle.ActualizarKilometraje(dto.KmOdometro);

        _context.FuelLoads.Add(fuelLoad);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), null,
            new FuelLoadDto(fuelLoad.Id, fuelLoad.VehicleId, vehicle.Placa, fuelLoad.Fecha,
                fuelLoad.Galones, fuelLoad.PrecioPorGalon, fuelLoad.Total, fuelLoad.KmOdometro,
                fuelLoad.Estacion, conductor?.NombreCompleto));
    }
}
