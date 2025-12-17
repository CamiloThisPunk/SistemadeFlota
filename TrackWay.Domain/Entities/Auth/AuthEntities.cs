using TrackWay.Domain.Common;

namespace TrackWay.Domain.Entities.Auth;

/// <summary>
/// Entidad de Usuario para autenticación
/// </summary>
public class User : Entity
{
    public string Email { get; private set; } = string.Empty;
    public string NombreCompleto { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }
    
    public bool Activo { get; private set; } = true;
    public bool EmailConfirmado { get; private set; } = false;
    public DateTime FechaCreacion { get; private set; }
    public DateTime? UltimoAcceso { get; private set; }
    public int IntentosLoginFallidos { get; private set; }
    public DateTime? BloqueoHasta { get; private set; }
    
    // Relaciones
    public int RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    
    private User() { }
    
    public static User Create(string email, string nombreCompleto, string passwordHash, int roleId)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email es requerido");
        
        return new User
        {
            Email = email.ToLower().Trim(),
            NombreCompleto = nombreCompleto,
            PasswordHash = passwordHash,
            RoleId = roleId,
            FechaCreacion = DateTime.UtcNow
        };
    }
    
    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
    }
    
    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiry = null;
    }
    
    public void RegistrarAccesoExitoso()
    {
        UltimoAcceso = DateTime.UtcNow;
        IntentosLoginFallidos = 0;
        BloqueoHasta = null;
    }
    
    public void RegistrarAccesoFallido()
    {
        IntentosLoginFallidos++;
        if (IntentosLoginFallidos >= 5)
        {
            BloqueoHasta = DateTime.UtcNow.AddMinutes(15);
        }
    }
    
    public bool EstaBloqueado => BloqueoHasta.HasValue && BloqueoHasta.Value > DateTime.UtcNow;
    
    public void Desactivar()
    {
        Activo = false;
        ClearRefreshToken();
    }
    
    public void ConfirmarEmail()
    {
        EmailConfirmado = true;
    }
    
    public void CambiarPassword(string nuevoPasswordHash)
    {
        PasswordHash = nuevoPasswordHash;
        ClearRefreshToken();
    }
}

/// <summary>
/// Entidad de Rol
/// </summary>
public class Role : Entity
{
    public string Nombre { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string Permisos { get; private set; } = string.Empty; // JSON array de permisos
    
    public bool Activo { get; private set; } = true;
    
    // Navegación
    private readonly List<User> _usuarios = new();
    public IReadOnlyCollection<User> Usuarios => _usuarios.AsReadOnly();
    
    private Role() { }
    
    public static Role Create(string nombre, string descripcion, string permisos = "[]")
    {
        return new Role
        {
            Nombre = nombre,
            Descripcion = descripcion,
            Permisos = permisos
        };
    }
}
