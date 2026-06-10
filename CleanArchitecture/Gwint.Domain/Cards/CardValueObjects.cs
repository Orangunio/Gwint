using Gwint.Domain.Common;

namespace Gwint.Domain.Cards;

/// <summary>
/// Tożsamość karty. Opakowanie identyfikatora w Value Object daje bezpieczeństwo
/// typów i jest nośnikiem tożsamości, przez którą agregaty komunikują się ze sobą.
/// </summary>
public sealed class CardId : ValueObject
{
    public int Value { get; }

    public CardId(int value)
    {
        if (value <= 0)
            throw new DomainException("CardId musi być dodatnią liczbą.");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}

/// <summary>Nazwa karty - pilnuje, że nigdy nie jest pusta.</summary>
public sealed class CardName : ValueObject
{
    public string Value { get; }

    public CardName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Nazwa karty nie może być pusta.");

        Value = value.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

/// <summary>Siła karty - pilnuje, że nie jest ujemna.</summary>
public sealed class CardStrength : ValueObject
{
    public int Value { get; }

    public CardStrength(int value)
    {
        if (value < 0)
            throw new DomainException("Siła karty nie może być ujemna.");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
