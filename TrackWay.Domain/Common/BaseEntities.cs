using MediatR;

namespace TrackWay.Domain.Common;

/// <summary>
/// Interface base para todas las entidades del dominio
/// </summary>
public interface IEntity
{
    int Id { get; }
}

/// <summary>
/// Interface para entidades que son raíces de agregado
/// </summary>
public interface IAggregateRoot : IEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

/// <summary>
/// Interface marcadora para eventos de dominio
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}

/// <summary>
/// Clase base abstracta para todas las entidades
/// </summary>
public abstract class Entity : IEntity
{
    public int Id { get; protected set; }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;
        
        if (ReferenceEquals(this, other))
            return true;
        
        if (GetType() != other.GetType())
            return false;
        
        if (Id == default || other.Id == default)
            return false;
        
        return Id == other.Id;
    }
    
    public static bool operator ==(Entity? a, Entity? b)
    {
        if (a is null && b is null)
            return true;
        
        if (a is null || b is null)
            return false;
        
        return a.Equals(b);
    }
    
    public static bool operator !=(Entity? a, Entity? b) => !(a == b);
    
    public override int GetHashCode() => (GetType().ToString() + Id).GetHashCode();
}

/// <summary>
/// Clase base para agregados que pueden emitir eventos de dominio
/// </summary>
public abstract class AggregateRoot : Entity, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

/// <summary>
/// Clase base abstracta para Value Objects
/// </summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;
        
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }
    
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }
    
    public static bool operator ==(ValueObject? a, ValueObject? b)
    {
        if (a is null && b is null)
            return true;
        
        if (a is null || b is null)
            return false;
        
        return a.Equals(b);
    }
    
    public static bool operator !=(ValueObject? a, ValueObject? b) => !(a == b);
}
