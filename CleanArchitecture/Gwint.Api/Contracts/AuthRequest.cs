namespace Gwint.Api.Contracts;

/// <summary>Body żądania rejestracji/logowania (kontrakt frontendu).</summary>
public sealed class AuthRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
