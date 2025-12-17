using TrackWay.Domain.Common;
using TrackWay.Domain.Enums;

namespace TrackWay.Domain.ValueObjects;

/// <summary>
/// Value Object para estado de alerta (Verde/Amarillo/Rojo)
/// </summary>
public sealed class AlertStatus : ValueObject
{
    public string Color { get; }
    public string Descripcion { get; }
    public int Prioridad { get; }
    
    private AlertStatus(string color, string descripcion, int prioridad)
    {
        Color = color;
        Descripcion = descripcion;
        Prioridad = prioridad;
    }
    
    public static AlertStatus Verde => new("Verde", "Todo en orden", 0);
    public static AlertStatus Amarillo => new("Amarillo", "Requiere atención pronto", 1);
    public static AlertStatus Rojo => new("Rojo", "Acción inmediata requerida", 2);
    
    public static AlertStatus FromDaysToExpiry(int days)
    {
        return days switch
        {
            > 30 => Verde,
            > 7 => Amarillo,
            _ => Rojo
        };
    }
    
    public static AlertStatus FromScore(decimal score)
    {
        return score switch
        {
            >= 80 => Verde,
            >= 50 => Amarillo,
            _ => Rojo
        };
    }
    
    public bool IsUrgent => Color == "Rojo";
    public bool RequiresAttention => Color != "Verde";
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Color;
    }
    
    public override string ToString() => $"{Color} - {Descripcion}";
}

/// <summary>
/// Value Object para categoría de licencia de conducir
/// </summary>
public sealed class LicenseCategory : ValueObject
{
    public CategoriaLicencia Categoria { get; }
    public string Codigo { get; }
    public string Descripcion { get; }
    public int PesoMaximoToneladas { get; }
    public int PasajerosMaximos { get; }
    
    private LicenseCategory(CategoriaLicencia categoria, string codigo, string descripcion, int pesoMax, int pasajerosMax)
    {
        Categoria = categoria;
        Codigo = codigo;
        Descripcion = descripcion;
        PesoMaximoToneladas = pesoMax;
        PasajerosMaximos = pasajerosMax;
    }
    
    public static LicenseCategory Create(CategoriaLicencia categoria)
    {
        return categoria switch
        {
            CategoriaLicencia.AI => new(categoria, "A-I", "Vehículos menores (motos)", 0, 1),
            CategoriaLicencia.AIIa => new(categoria, "A-IIa", "Automóviles hasta 8 pasajeros", 0, 8),
            CategoriaLicencia.AIIb => new(categoria, "A-IIb", "Automóviles hasta 16 pasajeros", 0, 16),
            CategoriaLicencia.AIIIa => new(categoria, "A-IIIa", "Transporte mercancías hasta 3.5 ton", 3, 0),
            CategoriaLicencia.AIIIb => new(categoria, "A-IIIb", "Transporte mercancías mayor 3.5 ton", 10, 0),
            CategoriaLicencia.AIIIc => new(categoria, "A-IIIc", "Transporte articulados/remolques", 30, 0),
            CategoriaLicencia.AIVA => new(categoria, "A-IVa", "Transporte personas", 0, 40),
            CategoriaLicencia.BIVB => new(categoria, "B-IVb", "Transporte especial", 0, 0),
            _ => throw new ArgumentException("Categoría de licencia no válida", nameof(categoria))
        };
    }
    
    public bool CanDrive(int vehicleWeight, int passengerCapacity)
    {
        if (PesoMaximoToneladas > 0 && vehicleWeight > PesoMaximoToneladas)
            return false;
        
        if (PasajerosMaximos > 0 && passengerCapacity > PasajerosMaximos)
            return false;
        
        return true;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Categoria;
    }
    
    public override string ToString() => $"{Codigo} - {Descripcion}";
}

/// <summary>
/// Value Object para kilometraje
/// </summary>
public sealed class Kilometraje : ValueObject
{
    public decimal Valor { get; }
    
    private Kilometraje(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentException("El kilometraje no puede ser negativo", nameof(valor));
        Valor = valor;
    }
    
    public static Kilometraje Create(decimal valor) => new(valor);
    public static Kilometraje Zero => new(0);
    
    public Kilometraje Add(decimal km) => new(Valor + km);
    
    public decimal DiferenciaDesde(Kilometraje otro) => Valor - otro.Valor;
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }
    
    public override string ToString() => $"{Valor:N0} km";
    
    public static implicit operator decimal(Kilometraje km) => km.Valor;
}

/// <summary>
/// Value Object para dinero/costo
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    private Money(decimal amount, string currency = "PEN")
    {
        if (amount < 0)
            throw new ArgumentException("El monto no puede ser negativo", nameof(amount));
        Amount = amount;
        Currency = currency;
    }
    
    public static Money Create(decimal amount, string currency = "PEN") => new(amount, currency);
    public static Money Zero => new(0);
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("No se pueden sumar montos de diferente moneda");
        return new(Amount + other.Amount, Currency);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
    
    public override string ToString() => $"{Currency} {Amount:N2}";
}
