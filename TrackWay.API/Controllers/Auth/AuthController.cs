using Microsoft.AspNetCore.Mvc;
using MediatR;
using TrackWay.Application.Auth.Commands;
using Microsoft.AspNetCore.Authorization;

namespace TrackWay.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Login - obtiene access y refresh tokens
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResult>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.Success)
            return Unauthorized(new { error = result.Error });
        
        return Ok(result);
    }

    /// <summary>
    /// Refresh token - renueva el access token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResult>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.Success)
            return Unauthorized(new { error = result.Error });
        
        return Ok(result);
    }

    /// <summary>
    /// Logout - invalida el refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return BadRequest();

        await _mediator.Send(new LogoutCommand(userId));
        return NoContent();
    }

    /// <summary>
    /// Registrar usuario - solo Admin
    /// </summary>
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RegisterResult>> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.Success)
            return BadRequest(new { error = result.Error });
        
        return CreatedAtAction(nameof(Login), new { id = result.UserId }, result);
    }

    /// <summary>
    /// Cambiar contraseña
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return BadRequest();

        var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
        var success = await _mediator.Send(command);
        
        if (!success)
            return BadRequest(new { error = "Contraseña actual incorrecta" });
        
        return NoContent();
    }

    /// <summary>
    /// Obtener información del usuario actual
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var nombre = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        return Ok(new
        {
            Id = userId,
            Email = email,
            NombreCompleto = nombre,
            Role = role
        });
    }
}

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
