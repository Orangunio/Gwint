using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gwint.Application.Abstractions.Security;
using Gwint.Domain.Players;
using Microsoft.IdentityModel.Tokens;

namespace Gwint.Infrastructure.Security;

/// <summary>
/// ADAPTER portu <see cref="IPasswordHasher"/> oparty o bibliotekę BCrypt.
/// Cała zależność od BCrypta jest zamknięta tutaj.
/// </summary>
internal sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plainPassword) => BCrypt.Net.BCrypt.HashPassword(plainPassword);

    public bool Verify(string plainPassword, string passwordHash) => BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);
}

/// <summary>Ustawienia JWT (wiązane z sekcją "Jwt" konfiguracji).</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "gwint-api";
    public string Audience { get; set; } = "gwint-client";
}

/// <summary>
/// ADAPTER portu <see cref="ITokenService"/>. Generuje token JWT z tymi samymi
/// claimami co dawny PlayerController (Name = login, "playerId" = Id), aby
/// zachować zgodność z istniejącym frontendem.
/// </summary>
internal sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(JwtOptions options) => _options = options;

    public string GenerateToken(Player player)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new InvalidOperationException("Brak klucza JWT w konfiguracji (Jwt:SecretKey).");

        var keyBytes = Encoding.UTF8.GetBytes(_options.SecretKey);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException(
                $"Klucz JWT jest za krótki ({keyBytes.Length} bajtów). Minimalna długość to 32 bajty.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, player.Login.Value),
                new Claim("playerId", player.Id.Value.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
