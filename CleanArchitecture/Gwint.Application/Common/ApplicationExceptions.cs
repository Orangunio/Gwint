namespace Gwint.Application.Common;

/// <summary>Login jest już zajęty (konflikt przy rejestracji).</summary>
public sealed class LoginAlreadyTakenException : Exception
{
    public LoginAlreadyTakenException(string login)
        : base("Użytkownik o takim loginie już istnieje.")
    {
    }
}

/// <summary>Nieprawidłowe dane logowania (zły login lub hasło).</summary>
public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException(string message) : base(message)
    {
    }
}

/// <summary>Nie znaleziono gracza.</summary>
public sealed class PlayerNotFoundException : Exception
{
    public PlayerNotFoundException(string message = "Gracz nie znaleziony.") : base(message)
    {
    }
}
