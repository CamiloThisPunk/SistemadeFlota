using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using TrackWay.Application.Common;
using TrackWay.Application.Fleet.Commands;
using TrackWay.Application.Fleet.Queries;
using TrackWay.Domain.Enums;

namespace TrackWay.API.Controllers.Fleet;

/// <summary>
/// Controller RESTful para Vehículos con alertas predictivas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Fleet - Vehículos")]
public class VehiclesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VehiclesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtener todos los vehículos con paginación
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<VehicleDto>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResult<VehicleDto>>>> GetAll([FromQuery] PagedRequest request)
    {
        var vehicles = await _mediator.Send(new GetAllVehiclesQuery());
        var paged = PagedResult<VehicleDto>.Create(vehicles, request.PageNumber, request.PageSize);
        return Ok(ApiResponse<PagedResult<VehicleDto>>.Ok(paged));
    }

    /// <summary>
    /// Obtener vehículo por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<VehicleDetailDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<ApiResponse<VehicleDetailDto>>> GetById(int id)
    {
        var vehicle = await _mediator.Send(new GetVehicleByIdQuery(id));
        if (vehicle == null)
            return NotFound(new ProblemDetails { Title = "No encontrado", Detail = $"Vehículo {id} no existe" });
        
        return Ok(ApiResponse<VehicleDetailDto>.Ok(vehicle));
    }

    /// <summary>
    /// Crear nuevo vehículo (Solo Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<CreateVehicleResult>), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<ActionResult<ApiResponse<CreateVehicleResult>>> Create([FromBody] CreateVehicleCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(ApiResponse<CreateVehicleResult>.Fail(result.Error ?? "Error al crear"));
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, 
            ApiResponse<CreateVehicleResult>.Ok(result, "Vehículo creado exitosamente"));
    }

    /// <summary>
    /// Actualizar kilometraje
    /// </summary>
    [HttpPatch("{id}/kilometraje")]
    [Authorize(Roles = "Admin,Gerente,Asistente")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateKilometraje(int id, [FromBody] decimal nuevoKm)
    {
        var result = await _mediator.Send(new UpdateVehicleKilometrajeCommand(id, nuevoKm));
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Vehículo no encontrado"));
        
        return Ok(ApiResponse<bool>.Ok(true, "Kilometraje actualizado"));
    }

    /// <summary>
    /// Cambiar estado del vehículo
    /// </summary>
    [HttpPatch("{id}/estado")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangeEstado(int id, [FromBody] EstadoOperativo nuevoEstado)
    {
        var result = await _mediator.Send(new ChangeVehicleEstadoCommand(id, nuevoEstado));
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Vehículo no encontrado"));
        
        return Ok(ApiResponse<bool>.Ok(true, $"Estado cambiado a {nuevoEstado}"));
    }

    /// <summary>
    /// Obtener alertas predictivas de vehículos (SOAT, mantenimiento, etc)
    /// </summary>
    [HttpGet("alertas")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<VehicleAlertDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<VehicleAlertDto>>>> GetAlertas([FromQuery] int diasAlerta = 30)
    {
        var alertas = await _mediator.Send(new GetVehiclesWithAlertsQuery(diasAlerta));
        return Ok(ApiResponse<IReadOnlyList<VehicleAlertDto>>.Ok(alertas, $"{alertas.Count} vehículos con alertas"));
    }

    /// <summary>
    /// Obtener vehículos por estado
    /// </summary>
    [HttpGet("por-estado/{estado}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<VehicleDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<VehicleDto>>>> GetByEstado(EstadoOperativo estado)
    {
        var vehicles = await _mediator.Send(new GetVehiclesByEstadoQuery(estado));
        return Ok(ApiResponse<IReadOnlyList<VehicleDto>>.Ok(vehicles));
    }

    /// <summary>
    /// Asignar conductor a vehículo
    /// </summary>
    [HttpPost("{vehicleId}/asignar-conductor/{driverId}")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> AsignarConductor(int vehicleId, int driverId)
    {
        var result = await _mediator.Send(new AssignDriverToVehicleCommand(vehicleId, driverId));
        if (!result)
            return BadRequest(ApiResponse<bool>.Fail("No se pudo asignar conductor"));
        
        return Ok(ApiResponse<bool>.Ok(true, "Conductor asignado exitosamente"));
    }

    /// <summary>
    /// Dar de baja vehículo (Solo Admin)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> Desactivar(int id)
    {
        var result = await _mediator.Send(new DeactivateVehicleCommand(id));
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Vehículo no encontrado"));
        
        return Ok(ApiResponse<bool>.Ok(true, "Vehículo dado de baja"));
    }
}
