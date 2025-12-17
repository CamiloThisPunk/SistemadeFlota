using MediatR;
using Microsoft.EntityFrameworkCore;
using TrackWay.Application.Auth.Commands;

namespace TrackWay.Application.Auth.Handlers;

/// <summary>
/// Handler para el comando de Login
/// </summary>
public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IAuthDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginHandler(IAuthDbContext context, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim(), cancellationToken);

        if (user == null)
            return new LoginResult(false, null, null, null, null, "Credenciales inválidas");

        if (!user.Activo)
            return new LoginResult(false, null, null, null, null, "Usuario inactivo");

        if (user.EstaBloqueado)
            return new LoginResult(false, null, null, null, null, "Usuario bloqueado temporalmente");

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RegistrarAccesoFallido();
            await _context.SaveChangesAsync(cancellationToken);
            return new LoginResult(false, null, null, null, null, "Credenciales inválidas");
        }

        // Login exitoso
        user.RegistrarAccesoExitoso();

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await _context.SaveChangesAsync(cancellationToken);

        var userInfo = new UserInfoDto(user.Id, user.Email, user.NombreCompleto, user.Role.Nombre);

        return new LoginResult(true, accessToken, refreshToken, expiresAt, userInfo, null);
    }
}

/// <summary>
/// Handler para el comando de Refresh Token
/// </summary>
public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, LoginResult>
{
    private readonly IAuthDbContext _context;
    private readonly IJwtService _jwtService;

    public RefreshTokenHandler(IAuthDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<LoginResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            return new LoginResult(false, null, null, null, null, "Token inválido");

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return new LoginResult(false, null, null, null, null, "Token inválido");

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || user.RefreshToken != request.RefreshToken || 
            user.RefreshTokenExpiry <= DateTime.UtcNow)
            return new LoginResult(false, null, null, null, null, "Refresh token inválido o expirado");

        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
        await _context.SaveChangesAsync(cancellationToken);

        var userInfo = new UserInfoDto(user.Id, user.Email, user.NombreCompleto, user.Role.Nombre);

        return new LoginResult(true, newAccessToken, newRefreshToken, expiresAt, userInfo, null);
    }
}

/// <summary>
/// Handler para el comando de Logout
/// </summary>
public class LogoutHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IAuthDbContext _context;

    public LogoutHandler(IAuthDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null)
            return false;

        user.ClearRefreshToken();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

/// <summary>
/// Handler para el comando de Registro
/// </summary>
public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterResult>
{
    private readonly IAuthDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(IAuthDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Verificar si el email ya existe
        var exists = await _context.Users.AnyAsync(u => u.Email == request.Email.ToLower().Trim(), cancellationToken);
        if (exists)
            return new RegisterResult(false, null, "El email ya está registrado");

        // Buscar el rol
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == request.RoleName, cancellationToken);
        if (role == null)
            return new RegisterResult(false, null, "Rol no encontrado");

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = Domain.Entities.Auth.User.Create(request.Email, request.NombreCompleto, passwordHash, role.Id);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegisterResult(true, user.Id, null);
    }
}

/// <summary>
/// Handler para cambio de contraseña
/// </summary>
public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IAuthDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordHandler(IAuthDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null)
            return false;

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            return false;

        var newHash = _passwordHasher.HashPassword(request.NewPassword);
        user.CambiarPassword(newHash);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

// Interfaces necesarias - deben implementarse en Infrastructure
public interface IAuthDbContext
{
    DbSet<Domain.Entities.Auth.User> Users { get; }
    DbSet<Domain.Entities.Auth.Role> Roles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public interface IJwtService
{
    string GenerateAccessToken(Domain.Entities.Auth.User user);
    string GenerateRefreshToken();
    System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
