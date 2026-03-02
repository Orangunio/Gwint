using Backend.Database;
using Backend.Migrations;
using Backend.Models.Body;
using Backend.Models.Enums;
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

        [HttpGet("get-player-fraction-deck/{playerId}/{fraction}")]
        public async Task<IActionResult> GetPlayerDecks(int playerId, int fraction)
        {
            Fractions fractionEnum;
            switch (fraction)
            {
                case 1:
                    fractionEnum = Fractions.Nilfgaard;
                    break;
                case 2:
                    fractionEnum = Fractions.NorthernRealms;
                    break;
                case 3:
                    fractionEnum = Fractions.ScoiaTael;
                    break;
                case 4:
                    fractionEnum = Fractions.Monsters;
                    break;
                default:
                    return BadRequest("Invalid fraction value.");
            }

            var playerDecks = await dbContext.PlayerDecks
                .Where(pd => pd.PlayerId == playerId && pd.Card.fraction == fractionEnum)
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
            Fractions fraction;
            switch (body.Fraction)
            {
                case 1:
                    fraction = Fractions.Nilfgaard;
                    break;
                case 2:
                    fraction = Fractions.NorthernRealms;
                    break;
                case 3:
                    fraction = Fractions.ScoiaTael;
                    break;
                case 4:
                    fraction = Fractions.Monsters;
                    break;
                default:
                    return BadRequest("Invalid fraction value.");
            }

            var existingCards = await dbContext.PlayerDecks
                .Where(pd => pd.PlayerId == body.PlayerId && pd.Card.fraction == fraction)
                .Select(pd => pd.CardId)
                .ToListAsync();

            var cardsToAdd = await dbContext.Cards
                .Where(c => body.CardIdsToAdd.Contains(c.Id) && c.fraction == fraction)
                .Select(c => c.Id)
                .Except(existingCards)
                .ToListAsync();

            foreach (var cardId in cardsToAdd)
            {
                dbContext.PlayerDecks.Add(new Models.PlayerDeck
                {
                    PlayerId = body.PlayerId,
                    CardId = cardId
                });
            }

            var cardsToRemove = await dbContext.PlayerDecks
                .Where(pd => pd.PlayerId == body.PlayerId
                             && pd.Card.fraction == fraction
                             && body.CardIdsToRemove.Contains(pd.CardId))
                .ToListAsync();

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
