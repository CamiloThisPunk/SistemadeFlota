using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TrackWay.Application.Common;
using TrackWay.Domain.Entities.Fleet;
using TrackWay.Domain.Enums;
using TrackWay.Infrastructure.Persistence;

namespace TrackWay.API.Controllers.Fleet;

/// <summary>
/// Controller RESTful para Órdenes de Mantenimiento con alertas predictivas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Fleet - Mantenimiento")]
public class MaintenanceController : ControllerBase
{
    private readonly TrackWayDbContext _context;

    public MaintenanceController(TrackWayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtener todos los mantenimientos con paginación
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<MaintenanceOrderDto>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResult<MaintenanceOrderDto>>>> GetAll([FromQuery] PagedRequest request)
    {
        var query = _context.MaintenanceOrders
            .Include(m => m.Vehicle)
            .Where(m => m.Estado != EstadoMantenimiento.Cancelado)
            .OrderByDescending(m => m.FechaProgramada)
            .Select(m => new MaintenanceOrderDto(
                m.Id,
                m.Vehicle!.Placa,
                m.Tipo.ToString(),
                m.Estado.ToString(),
                m.Descripcion,
                m.FechaProgramada,
                m.CostoEstimado,
                m.CostoReal
            ));

        var total = await query.CountAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var paged = new PagedResult<MaintenanceOrderDto>(items, total, request.PageNumber, request.PageSize);
        return Ok(ApiResponse<PagedResult<MaintenanceOrderDto>>.Ok(paged));
    }

    /// <summary>
    /// Obtener mantenimiento por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<MaintenanceOrder>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<ApiResponse<MaintenanceOrder>>> GetById(int id)
    {
        var order = await _context.MaintenanceOrders
            .Include(m => m.Vehicle)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (order == null)
            return NotFound(new ProblemDetails { Title = "No encontrado", Detail = $"Orden {id} no existe" });

        return Ok(ApiResponse<MaintenanceOrder>.Ok(order));
    }

    /// <summary>
    /// Crear orden de mantenimiento
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<int>), 201)]
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] CreateMaintenanceRequest request)
    {
        try
        {
            var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
            if (vehicle == null)
                return BadRequest(ApiResponse<int>.Fail("Vehículo no encontrado"));

            // Verificar que el vehículo esté activo
            if (!vehicle.Activo)
                return BadRequest(ApiResponse<int>.Fail("El vehículo está dado de baja y no puede recibir órdenes de mantenimiento"));

            var order = MaintenanceOrder.Create(
                request.VehicleId,
                request.Tipo,
                request.Descripcion,
                request.FechaProgramada,
                vehicle.KmActual,
                request.CostoEstimado
            );

            _context.MaintenanceOrders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = order.Id },
                ApiResponse<int>.Ok(order.Id, "Orden de mantenimiento creada"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<int>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<int>.Fail($"Error interno: {ex.Message}"));
        }
    }

    /// <summary>
    /// Iniciar mantenimiento (cambiar a EnTaller)
    /// </summary>
    [HttpPost("{id}/iniciar")]
    [Authorize(Roles = "Admin,Gerente,Asistente")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> Iniciar(int id, [FromBody] string? proveedor)
    {
        var order = await _context.MaintenanceOrders.FindAsync(id);
        if (order == null)
            return NotFound(ApiResponse<bool>.Fail("Orden no encontrada"));

        if (proveedor != null)
            order.AsignarProveedor(proveedor);
        order.IniciarTrabajo();
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Mantenimiento iniciado"));
    }

    /// <summary>
    /// Finalizar mantenimiento
    /// </summary>
    [HttpPost("{id}/finalizar")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> Finalizar(int id, [FromBody] FinalizarMaintenanceRequest request)
    {
        var order = await _context.MaintenanceOrders.FindAsync(id);
        if (order == null)
            return NotFound(ApiResponse<bool>.Fail("Orden no encontrada"));

        order.Finalizar(request.CostoReal, request.Observaciones);
        if (request.KmProximo.HasValue)
            order.EstablecerProximoMantenimiento(request.KmProximo.Value);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Mantenimiento finalizado"));
    }

    /// <summary>
    /// Obtener alertas de mantenimiento (vencidos + por km)
    /// </summary>
    [HttpGet("alertas")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AlertaPredictivaDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AlertaPredictivaDto>>>> GetAlertas()
    {
        var alertas = new List<AlertaPredictivaDto>();

        // Mantenimientos vencidos
        var vencidos = await _context.MaintenanceOrders
            .Include(m => m.Vehicle)
            .Where(m => m.FechaProgramada < DateTime.UtcNow && 
                       (m.Estado == EstadoMantenimiento.Pendiente || m.Estado == EstadoMantenimiento.Programado))
            .ToListAsync();

        foreach (var m in vencidos)
        {
            alertas.Add(new AlertaPredictivaDto(
                "Mantenimiento",
                $"Vehículo {m.Vehicle?.Placa}",
                m.Id,
                $"Mantenimiento {m.Tipo} vencido desde {m.FechaProgramada:dd/MM/yyyy}",
                "Critical",
                m.FechaProgramada,
                (int)(DateTime.UtcNow - m.FechaProgramada).TotalDays * -1,
                "Programar cita de mantenimiento inmediatamente"
            ));
        }

        // Vehículos con km cercano a próximo mantenimiento
        var vehiculosConKm = await _context.Vehicles
            .Include(v => v.OrdenesMantenimiento)
            .Where(v => v.Activo && v.OrdenesMantenimiento.Any(m => 
                m.KmProximoMantenimiento.HasValue && 
                v.KmActual >= m.KmProximoMantenimiento.Value - 500))
            .ToListAsync();

        foreach (var v in vehiculosConKm)
        {
            var proximoMant = v.OrdenesMantenimiento
                .Where(m => m.KmProximoMantenimiento.HasValue)
                .OrderBy(m => m.KmProximoMantenimiento)
                .FirstOrDefault();

            if (proximoMant != null)
            {
                var kmRestantes = proximoMant.KmProximoMantenimiento!.Value - v.KmActual;
                alertas.Add(new AlertaPredictivaDto(
                    "Mantenimiento por Km",
                    $"Vehículo {v.Placa}",
                    v.Id,
                    $"Próximo mantenimiento en {kmRestantes:N0} km",
                    kmRestantes <= 0 ? "Critical" : "Warning",
                    null,
                    null,
                    $"Programar cambio de {proximoMant.Tipo}"
                ));
            }
        }

        return Ok(ApiResponse<IReadOnlyList<AlertaPredictivaDto>>.Ok(alertas, $"{alertas.Count} alertas activas"));
    }

    /// <summary>
    /// Obtener mantenimientos pendientes por vehículo
    /// </summary>
    [HttpGet("vehiculo/{vehicleId}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MaintenanceOrderDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MaintenanceOrderDto>>>> GetByVehicle(int vehicleId)
    {
        var orders = await _context.MaintenanceOrders
            .Include(m => m.Vehicle)
            .Where(m => m.VehicleId == vehicleId && m.Estado != EstadoMantenimiento.Cancelado)
            .OrderByDescending(m => m.FechaProgramada)
            .Select(m => new MaintenanceOrderDto(
                m.Id,
                m.Vehicle!.Placa,
                m.Tipo.ToString(),
                m.Estado.ToString(),
                m.Descripcion,
                m.FechaProgramada,
                m.CostoEstimado,
                m.CostoReal
            ))
            .ToListAsync();

        return Ok(ApiResponse<IReadOnlyList<MaintenanceOrderDto>>.Ok(orders));
    }
}

// DTOs
public record MaintenanceOrderDto(
    int Id,
    string VehiculoPlaca,
    string Tipo,
    string Estado,
    string Descripcion,
    DateTime FechaProgramada,
    decimal CostoEstimado,
    decimal? CostoReal);

public record CreateMaintenanceRequest(
    int VehicleId,
    TipoMantenimiento Tipo,
    string Descripcion,
    DateTime FechaProgramada,
    decimal CostoEstimado);

public record FinalizarMaintenanceRequest(
    decimal CostoReal,
    string? Observaciones,
    decimal? KmProximo);
