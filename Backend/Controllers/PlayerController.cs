using Backend.Database;
using Backend.Models;
using Backend.UseCases;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/player")]
    public class PlayerController : ControllerBase
    {
        public readonly GwintDBContext _dbContext;
        public PlayerController(GwintDBContext _dbContext)
        {
            this._dbContext = _dbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> CreatePlayer([FromBody] Models.Body.AuthBody request)
        {
            PlayerUseCases playerUseCases = new PlayerUseCases();
            Player player = playerUseCases.CreatePlayer(request.Login, request.Password);

            var login = _dbContext.Players.FirstOrDefault(p => p.Login == request.Login);
            if (login != null)
            {
                return BadRequest("Użytkownik o takim loginie już istnieje.");
            }

            _dbContext.Players.Add(player);
            await _dbContext.SaveChangesAsync();
            return new OkObjectResult(player);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Models.Body.AuthBody request, [FromServices] IConfiguration configuration)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Login i hasło są wymagane.");
            }

            var player = await _dbContext.Players
                .FirstOrDefaultAsync(p => p.Login == request.Login);

            if (player == null)
            {
                return BadRequest("Użytkownik nie został znaleziony.");
            }

            var playerUseCases = new PlayerUseCases();
            if (!playerUseCases.VerifyPassword(request.Password, player.HashPassword))
            {
                return BadRequest("Nieprawidłowe hasło.");
            }

            var secretKey = configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Brak klucza JWT w konfiguracji (Jwt:SecretKey)");

            var keyBytes = Encoding.UTF8.GetBytes(secretKey);

            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    $"Klucz JWT jest za krótki ({keyBytes.Length} bajtów). Minimalna długość to 32 bajty (256 bitów).");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, player.Login),
                    new Claim("playerId", player.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = configuration["Jwt:Issuer"] ?? "gwint-api",
                Audience = configuration["Jwt:Audience"] ?? "gwint-client",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return Ok(new { token = jwt });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var playerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "playerId")?.Value;
            
            if (!int.TryParse(playerIdClaim, out var playerId))
                return Unauthorized("Nieprawidłowy token.");
        
            var player = await _dbContext.Players
                .FirstOrDefaultAsync(p => p.Id == playerId);
        
            if (player == null)
                return NotFound("Gracz nie znaleziony.");
        
            return Ok(new { player.Id, player.Login });
        }
        
    }

    
}