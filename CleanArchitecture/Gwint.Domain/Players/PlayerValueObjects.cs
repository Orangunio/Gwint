using Gwint.Domain.Common;

namespace Gwint.Domain.Players;

/// <summary>
/// Tożsamość gracza. Wartość 0 oznacza gracza jeszcze nieutrwalonego
/// (identyfikator nada baza danych przy zapisie).
/// </summary>
public sealed class PlayerId : ValueObject
{
    public int Value { get; }

    public PlayerId(int value)
    {
        if (value < 0)
            throw new DomainException("PlayerId nie może być ujemne.");

        Value = value;
    }

    /// <summary>Tożsamość gracza, który nie został jeszcze zapisany w bazie.</summary>
    public static PlayerId Unassigned => new(0);

    public bool IsAssigned => Value > 0;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}

/// <summary>
/// Login gracza - Value Object z regułami poprawności (niepusty, sensowna długość).
/// Reguły, które dawniej były w kontrolerze, mieszkają teraz w jednym miejscu.
/// </summary>
public sealed class Login : ValueObject
{
    public const int MaxLength = 100;

    public string Value { get; }

    public Login(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Login jest wymagany.");

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new DomainException($"Login może mieć maksymalnie {MaxLength} znaków.");

        Value = trimmed;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

/// <summary>
/// Zahaszowane hasło - domena NIGDY nie przechowuje hasła jawnego. Samo
/// hashowanie to szczegół (BCrypt) ukryty za portem IPasswordHasher - tutaj
/// trzymamy wyłącznie gotowy, niepusty hash.
/// </summary>
public sealed class HashedPassword : ValueObject
{
    public string Value { get; }

    public HashedPassword(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Hash hasła nie może być pusty.");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
