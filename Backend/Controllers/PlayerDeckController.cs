using Backend.Database;
using Backend.Migrations;
using Backend.Models.Body;
using Backend.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/player-deck")]
    public class PlayerDeckController : ControllerBase
    {
        public readonly GwintDBContext dbContext;

        public PlayerDeckController(GwintDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("get-player-deck/{playerId}")]
        public async Task<IActionResult> GetPlayerDecks(int playerId)
        {
            var playerDecks = await dbContext.PlayerDecks
                .Where(pd => pd.PlayerId == playerId)
                .Select(pd => new
                {
                    pd.Id,
                    pd.CardId,
                    CardName = pd.Card.Name
                })
                .ToListAsync();
            return Ok(playerDecks);
        }

        [HttpPost("update-deck")]
        public async Task<IActionResult> UpdateDeck([FromBody] UpdateDeckBody body)
        {
            var playerDecks = await dbContext.PlayerDecks
                .Where(pd => pd.PlayerId == body.PlayerId)
                .ToListAsync();

            foreach (var cardId in body.CardIdsToAdd.Except(playerDecks.Select(pd => pd.CardId)))
            {
                dbContext.PlayerDecks.Add(new Models.PlayerDeck
                {
                    PlayerId = body.PlayerId,
                    CardId = cardId
                });
            }

            var cardsToRemove = playerDecks
                .Where(pd => body.CardIdsToRemove.Contains(pd.CardId))
                .ToList();

            dbContext.PlayerDecks.RemoveRange(cardsToRemove);

            await dbContext.SaveChangesAsync();

            var updatedDeck = await dbContext.PlayerDecks
                .Where(pd => pd.PlayerId == body.PlayerId)
                .Include(pd => pd.Card)
                .ToListAsync();

            return Ok(updatedDeck);
        }
    }
}
