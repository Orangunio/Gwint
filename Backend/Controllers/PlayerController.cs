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
    }
}