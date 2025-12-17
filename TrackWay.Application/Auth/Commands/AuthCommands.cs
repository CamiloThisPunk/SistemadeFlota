using MediatR;

namespace TrackWay.Application.Auth.Commands;

/// <summary>
/// Comando para login
/// </summary>
public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public record LoginResult(
    bool Success,
    string? AccessToken,
    string? RefreshToken,
    DateTime? ExpiresAt,
    UserInfoDto? User,
    string? Error);

public record UserInfoDto(
    int Id,
    string Email,
    string NombreCompleto,
    string Role);

/// <summary>
/// Comando para refresh token
/// </summary>
public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<LoginResult>;

/// <summary>
/// Comando para logout
/// </summary>
public record LogoutCommand(int UserId) : IRequest<bool>;

/// <summary>
/// Comando para registrar usuario (solo Admin)
/// </summary>
public record RegisterUserCommand(
    string Email,
    string NombreCompleto,
    string Password,
    string RoleName
) : IRequest<RegisterResult>;

public record RegisterResult(bool Success, int? UserId, string? Error);

/// <summary>
/// Comando para cambiar password
/// </summary>
public record ChangePasswordCommand(
    int UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<bool>;
