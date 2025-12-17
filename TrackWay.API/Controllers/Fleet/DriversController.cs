using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using TrackWay.Application.Common;
using TrackWay.Application.Fleet.Commands;
using TrackWay.Application.Fleet.Queries;

namespace TrackWay.API.Controllers.Fleet;

/// <summary>
/// Controller RESTful para Conductores con alertas de licencia
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Fleet - Conductores")]
public class DriversController : ControllerBase
{
    private readonly IMediator _mediator;

    public DriversController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtener todos los conductores con paginación
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DriverDto>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverDto>>>> GetAll([FromQuery] PagedRequest request)
    {
        var drivers = await _mediator.Send(new GetAllDriversQuery());
        var paged = PagedResult<DriverDto>.Create(drivers, request.PageNumber, request.PageSize);
        return Ok(ApiResponse<PagedResult<DriverDto>>.Ok(paged));
    }

    /// <summary>
    /// Obtener conductor por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DriverDetailDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<ApiResponse<DriverDetailDto>>> GetById(int id)
    {
        var driver = await _mediator.Send(new GetDriverByIdQuery(id));
        if (driver == null)
            return NotFound(new ProblemDetails { Title = "No encontrado", Detail = $"Conductor {id} no existe" });
        
        return Ok(ApiResponse<DriverDetailDto>.Ok(driver));
    }

    /// <summary>
    /// Crear nuevo conductor (Solo Admin/Gerente)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<CreateDriverResult>), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<ActionResult<ApiResponse<CreateDriverResult>>> Create([FromBody] CreateDriverCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(ApiResponse<CreateDriverResult>.Fail(result.Error ?? "Error al crear"));
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, 
            ApiResponse<CreateDriverResult>.Ok(result, "Conductor creado exitosamente"));
    }

    /// <summary>
    /// Actualizar scoring de seguridad
    /// </summary>
    [HttpPatch("{id}/scoring")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateScoring(int id, [FromBody] decimal nuevoScore)
    {
        var result = await _mediator.Send(new UpdateDriverScoringCommand(id, nuevoScore));
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Conductor no encontrado"));
        
        return Ok(ApiResponse<bool>.Ok(true, "Scoring actualizado"));
    }

    /// <summary>
    /// Renovar licencia de conducir
    /// </summary>
    [HttpPatch("{id}/renovar-licencia")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> RenovarLicencia(int id, [FromBody] DateTime nuevaFechaVencimiento)
    {
        var result = await _mediator.Send(new RenewDriverLicenseCommand(id, nuevaFechaVencimiento));
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Conductor no encontrado"));
        
        return Ok(ApiResponse<bool>.Ok(true, "Licencia renovada exitosamente"));
    }

    /// <summary>
    /// Obtener alertas de licencias por vencer
    /// </summary>
    [HttpGet("alertas/licencia")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DriverAlertDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DriverAlertDto>>>> GetAlertasLicencia([FromQuery] int diasAlerta = 30)
    {
        var alertas = await _mediator.Send(new GetDriversWithLicenseAlertsQuery(diasAlerta));
        return Ok(ApiResponse<IReadOnlyList<DriverAlertDto>>.Ok(alertas, $"{alertas.Count} licencias por vencer"));
    }

    /// <summary>
    /// Obtener conductores disponibles (sin vehículo asignado)
    /// </summary>
    [HttpGet("disponibles")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DriverDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DriverDto>>>> GetDisponibles()
    {
        var drivers = await _mediator.Send(new GetAvailableDriversQuery());
        return Ok(ApiResponse<IReadOnlyList<DriverDto>>.Ok(drivers));
    }
}
