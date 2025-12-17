using TrackWay.Domain.Common;
using TrackWay.Domain.Enums;
using TrackWay.Domain.Events;
using TrackWay.Domain.ValueObjects;

namespace TrackWay.Domain.Entities.Fleet;

/// <summary>
/// Entidad Conductor/Chofer - Aggregate Root
/// </summary>
public class Driver : AggregateRoot
{
    // Datos personales
    public string Nombre { get; private set; } = string.Empty;
    public string Apellidos { get; private set; } = string.Empty;
    public string Documento { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Telefono { get; private set; } = string.Empty;
    public string? FotoUrl { get; private set; }
    
    // Licencia
    public string LicenciaNum { get; private set; } = string.Empty;
    public CategoriaLicencia CategoriaLicencia { get; private set; }
    public DateTime LicenciaVencimiento { get; private set; }
    
    // Métricas
    public decimal ScoringSeguridad { get; private set; } = 100;
    public int TotalViajes { get; private set; }
    public decimal KmTotalesRecorridos { get; private set; }
    
    // Estado
    public bool Activo { get; private set; } = true;
    public DateTime FechaContratacion { get; private set; }
    public DateTime? FechaBaja { get; private set; }
    
    // Relaciones
    public int? VehiculoAsignadoId { get; private set; }
    public Vehicle? VehiculoAsignado { get; private set; }
    
    // Constructor privado para EF Core
    private Driver() { }
    
    // Factory method
    public static Driver Create(
        string nombre,
        string apellidos,
        string documento,
        string licenciaNum,
        CategoriaLicencia categoria,
        DateTime licenciaVencimiento)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es requerido", nameof(nombre));
        
        if (licenciaVencimiento <= DateTime.UtcNow)
            throw new ArgumentException("La licencia ya está vencida", nameof(licenciaVencimiento));
        
        var driver = new Driver
        {
            Nombre = nombre,
            Apellidos = apellidos,
            Documento = documento,
            LicenciaNum = licenciaNum.ToUpper().Trim(),
            CategoriaLicencia = categoria,
            LicenciaVencimiento = licenciaVencimiento,
            FechaContratacion = DateTime.UtcNow,
            ScoringSeguridad = 100
        };
        
        driver.AddDomainEvent(new DriverCreatedEvent(driver.Id, $"{nombre} {apellidos}", licenciaNum));
        
        return driver;
    }
    
    // Métodos de dominio
    public void ActualizarScoring(decimal nuevoScore)
    {
        if (nuevoScore < 0 || nuevoScore > 100)
            throw new ArgumentException("El score debe estar entre 0 y 100", nameof(nuevoScore));
        
        var oldScore = ScoringSeguridad;
        ScoringSeguridad = nuevoScore;
        
        AddDomainEvent(new DriverScoreUpdatedEvent(Id, oldScore, nuevoScore));
    }
    
    public void RegistrarViaje(decimal kmRecorridos)
    {
        TotalViajes++;
        KmTotalesRecorridos += kmRecorridos;
    }
    
    public void RenovarLicencia(DateTime nuevaFechaVencimiento)
    {
        if (nuevaFechaVencimiento <= DateTime.UtcNow)
            throw new ArgumentException("La fecha de vencimiento debe ser futura");
        
        LicenciaVencimiento = nuevaFechaVencimiento;
    }
    
    public void DarDeBaja()
    {
        Activo = false;
        FechaBaja = DateTime.UtcNow;
        VehiculoAsignadoId = null;
        VehiculoAsignado = null;
    }
    
    public AlertStatus GetAlertaLicencia()
    {
        var diasRestantes = (LicenciaVencimiento - DateTime.UtcNow).Days;
        
        if (diasRestantes <= 0)
        {
            AddDomainEvent(new DriverLicenseExpiringEvent(Id, NombreCompleto, LicenciaVencimiento, diasRestantes));
        }
        
        return AlertStatus.FromDaysToExpiry(diasRestantes);
    }
    
    public AlertStatus GetAlertaSeguridad() => AlertStatus.FromScore(ScoringSeguridad);
    
    public bool PuedeConducir(Vehicle vehiculo)
    {
        if (!Activo) return false;
        if (LicenciaVencimiento <= DateTime.UtcNow) return false;
        if (ScoringSeguridad < 50) return false;
        
        var licencia = LicenseCategory.Create(CategoriaLicencia);
        return licencia.CanDrive(0, 0); // Simplificado
    }
    
    public string NombreCompleto => $"{Nombre} {Apellidos}";
    
    public int DiasParaVencimientoLicencia => Math.Max(0, (LicenciaVencimiento - DateTime.UtcNow).Days);
    
    public bool LicenciaVigente => LicenciaVencimiento > DateTime.UtcNow;
}
