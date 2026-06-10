namespace Gwint.Domain.Common;

/// <summary>
/// Wyjątek sygnalizujący złamanie reguły biznesowej (niezmiennika) domeny.
/// Domena pilnuje swoich niezmienników sama - zamiast pozwalać na utworzenie
/// obiektu w niepoprawnym stanie, rzuca ten wyjątek.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

/// <summary>
/// Bazowa klasa Obiektu Wartości (Value Object).
/// Obiekty wartości są NIEMUTOWALNE i porównywane przez wartość swoich
/// składowych (a nie przez tożsamość). Dwa obiekty wartości o takich samych
/// składowych są sobie równe.
/// </summary>
public abstract class ValueObject
{
    /// <summary>Składowe decydujące o równości obiektu wartości.</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var component in GetEqualityComponents())
            hash.Add(component);
        return hash.ToHashCode();
    }

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}

/// <summary>
/// Bazowa klasa Encji (Entity). Encja ma TOŻSAMOŚĆ (Id) - dwie encje są równe
/// wtedy i tylko wtedy, gdy mają to samo Id, niezależnie od pozostałych
/// atrybutów. Stan zmieniają wyłącznie przez własne metody pilnujące niezmienników.
/// </summary>
/// <typeparam name="TId">Typ tożsamości (zwykle Value Object Id).</typeparam>
public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; }

    protected Entity(TId id) => Id = id;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other || obj.GetType() != GetType())
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();
}
