using Gwint.Application.Abstractions.Persistence;
using Gwint.Application.Abstractions.Security;
using Gwint.Application.Common;
using Gwint.Domain.Players;

namespace Gwint.Application.Players;

/// <summary>
/// Przypadki użycia kontekstu uwierzytelniania (rejestracja, logowanie,
/// pobranie własnych danych). Orkiestruje domenę i porty - nie zna technologii.
/// </summary>
public sealed class AuthService
{
    private readonly IPlayerRepository _players;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IPlayerRepository players,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _players = players;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    /// <summary>Przypadek użycia: rejestracja nowego gracza.</summary>
    public async Task<PlayerDto> RegisterAsync(string login, string password, CancellationToken ct = default)
    {
        var loginVo = new Login(login);

        if (await _players.ExistsByLoginAsync(loginVo, ct))
            throw new LoginAlreadyTakenException(loginVo.Value);

        var hashed = new HashedPassword(_passwordHasher.Hash(password));
        var player = Player.Register(loginVo, hashed);

        var saved = await _players.AddAsync(player, ct);
        return new PlayerDto(saved.Id.Value, saved.Login.Value);
    }

    /// <summary>Przypadek użycia: logowanie i wydanie tokenu.</summary>
    public async Task<string> LoginAsync(string login, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            throw new InvalidCredentialsException("Login i hasło są wymagane.");

        var player = await _players.GetByLoginAsync(new Login(login), ct);
        if (player is null)
            throw new InvalidCredentialsException("Użytkownik nie został znaleziony.");

        if (!_passwordHasher.Verify(password, player.Password.Value))
            throw new InvalidCredentialsException("Nieprawidłowe hasło.");

        return _tokenService.GenerateToken(player);
    }

    /// <summary>Przypadek użycia: pobranie danych gracza po identyfikatorze.</summary>
    public async Task<PlayerDto> GetByIdAsync(int playerId, CancellationToken ct = default)
    {
        var player = await _players.GetByIdAsync(new PlayerId(playerId), ct);
        if (player is null)
            throw new PlayerNotFoundException();

        return new PlayerDto(player.Id.Value, player.Login.Value);
    }
}
