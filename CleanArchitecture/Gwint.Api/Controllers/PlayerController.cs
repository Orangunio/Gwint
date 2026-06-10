using Gwint.Api.Contracts;
using Gwint.Application.Common;
using Gwint.Application.Players;
using Gwint.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gwint.Api.Controllers;

/// <summary>
/// Cienki kontroler uwierzytelniania. Nie zawiera logiki biznesowej - jedynie
/// przyjmuje żądanie, woła przypadek użycia (AuthService) i tłumaczy wynik/wyjątki
/// na odpowiedzi HTTP zgodne z dotychczasowym kontraktem frontendu.
/// </summary>
[ApiController]
[Route("api/player")]
public sealed class PlayerController : ControllerBase
{
    private readonly AuthService _auth;

    public PlayerController(AuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRequest request, CancellationToken ct)
    {
        try
        {
            var player = await _auth.RegisterAsync(request.Login, request.Password, ct);
            return Ok(new { player.Id, player.Login });
        }
        catch (LoginAlreadyTakenException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest request, CancellationToken ct)
    {
        try
        {
            var token = await _auth.LoginAsync(request.Login, request.Password, ct);
            return Ok(new { token });
        }
        catch (InvalidCredentialsException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var playerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "playerId")?.Value;

        if (!int.TryParse(playerIdClaim, out var playerId))
            return Unauthorized("Nieprawidłowy token.");

        try
        {
            var player = await _auth.GetByIdAsync(playerId, ct);
            return Ok(new { player.Id, player.Login });
        }
        catch (PlayerNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
