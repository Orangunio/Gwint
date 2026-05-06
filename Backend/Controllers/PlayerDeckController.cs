using Backend.Database;
using Backend.Models.Body;
using Backend.Models.Enums;
using Backend.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    //[Authorize]
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
                    CardName = pd.Card.Name,
                    Fraction = (int)pd.Card.fraction,
                    Ability = (int)pd.Card.ability,
                    Strength = pd.Card.Strength,
                    Place = (int)pd.Card.place,
                    IsChampion = pd.Card.isChampion,
                    IsCommander = pd.Card.isCommander,
                    IsSpecial = pd.Card.isSpecial,
                })
                .ToListAsync();

            return Ok(playerDecks);
        }

        [HttpGet("get-player-fraction-deck/{playerId}/{fraction}")]
        public async Task<IActionResult> GetPlayerFractionDeck(int playerId, int fraction)
        {
            if (fraction < 1 || fraction > 4)
                return BadRequest("Nieprawidłowa frakcja.");

            Fractions fractionEnum = fraction switch
            {
                1 => Fractions.Nilfgaard,
                2 => Fractions.NorthernRealms,
                3 => Fractions.ScoiaTael,
                4 => Fractions.Monsters,
                _ => throw new ArgumentException("Invalid fraction")
            };

            try
            {
                var playerCards = await dbContext.PlayerDecks
                    .Include(pd => pd.Card)
                    .Where(pd => pd.PlayerId == playerId
                              && pd.Card.fraction == fractionEnum)
                    .Select(pd => new
                    {
                        pd.Id,
                        CardId = pd.CardId,
                        CardName = pd.Card.Name,
                        Fraction = (int)pd.Card.fraction,
                        Ability = (int)pd.Card.ability,
                        Strength = pd.Card.Strength,
                        Place = (int)pd.Card.place,
                        IsChampion = pd.Card.isChampion,
                        IsCommander = pd.Card.isCommander,
                        IsSpecial = pd.Card.isSpecial,
                    })
                    .ToListAsync();

                return Ok(playerCards);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetPlayerFractionDeck({playerId}, {fraction}): {ex}");
                // Zwróć więcej informacji podczas developmentu
                return StatusCode(500, new
                {
                    error = "Błąd pobierania talii",
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPost("update-deck")]
        public async Task<IActionResult> UpdateDeck([FromBody] UpdateDeckBody body)
        {
            Fractions fraction;
            switch (body.Fraction)
            {
                case 1: fraction = Fractions.Nilfgaard; break;
                case 2: fraction = Fractions.NorthernRealms; break;
                case 3: fraction = Fractions.ScoiaTael; break;
                case 4: fraction = Fractions.Monsters; break;
                default: return BadRequest("Invalid fraction value.");
            }

            try
            {
                // Pobierz aktualne karty gracza w danej frakcji
                var currentDeckEntries = await dbContext.PlayerDecks
                    .Include(pd => pd.Card)
                    .Where(pd => pd.PlayerId == body.PlayerId && pd.Card.fraction == fraction)
                    .ToListAsync();

                // Usuń karty oznaczone do usunięcia
                var cardsToRemove = currentDeckEntries
                    .Where(pd => body.CardIdsToRemove.Contains(pd.CardId))
                    .ToList();

                dbContext.PlayerDecks.RemoveRange(cardsToRemove);

                // Pozostałe karty po usunięciu
                var remainingCardIds = currentDeckEntries
                    .Where(pd => !body.CardIdsToRemove.Contains(pd.CardId))
                    .Select(pd => pd.CardId)
                    .ToHashSet();

                // Karty do dodania (tylko te, których jeszcze nie ma)
                var cardsToAddIds = body.CardIdsToAdd
                    .Where(id => !remainingCardIds.Contains(id))
                    .Distinct()
                    .ToList();

                // Pobierz pełne obiekty kart do dodania
                var cardsToAddFull = await dbContext.Cards
                    .Where(c => cardsToAddIds.Contains(c.Id) && c.fraction == fraction)
                    .ToListAsync();

                // Walidacja talii (zachowane Twoje oryginalne warunki)
                var finalCards = currentDeckEntries
                    .Where(pd => !body.CardIdsToRemove.Contains(pd.CardId))
                    .Select(pd => pd.Card)
                    .Concat(cardsToAddFull)
                    .ToList();

                var commandersCount = finalCards.Count(c => c.isCommander);
                if (commandersCount != 1)
                    return BadRequest("Talia musi zawierać dokładnie jednego dowódcę.");

                var specialCardsCount = finalCards.Count(c => c.isSpecial);
                if (specialCardsCount > 10)
                    return BadRequest("Talia może zawierać maksymalnie 10 kart specjalnych.");

                if (finalCards.Count < 26)
                    return BadRequest("Talia musi zawierać minimum 25 kart bez dowódcy");

                // Dodaj nowe karty
                foreach (var card in cardsToAddFull)
                {
                    dbContext.PlayerDecks.Add(new Models.PlayerDeck
                    {
                        PlayerId = body.PlayerId,
                        CardId = card.Id
                    });
                }

                await dbContext.SaveChangesAsync();

                // Zwróć zaktualizowaną talię
                var updatedDeck = await dbContext.PlayerDecks
                    .Include(pd => pd.Card)
                    .Where(pd => pd.PlayerId == body.PlayerId && pd.Card.fraction == fraction)
                    .Select(pd => new
                    {
                        pd.Id,
                        CardId = pd.CardId,
                        CardName = pd.Card.Name,
                        Fraction = (int)pd.Card.fraction,
                        Ability = (int)pd.Card.ability,
                        Strength = pd.Card.Strength,
                        Place = (int)pd.Card.place,
                        IsChampion = pd.Card.isChampion,
                        IsCommander = pd.Card.isCommander,
                        IsSpecial = pd.Card.isSpecial,
                    })
                    .ToListAsync();

                return Ok(updatedDeck);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdateDeck: {ex}");
                return StatusCode(500, new
                {
                    error = "Błąd zapisu talii",
                    message = ex.Message
                });
            }
        }

        [HttpGet("available-cards/{fraction}")]

        public async Task<IActionResult> GetAvailableCards(int fraction)
        {
            Fractions fractionEnum = fraction switch
            {
                1 => Fractions.Nilfgaard,
                2 => Fractions.NorthernRealms,
                3 => Fractions.ScoiaTael,
                4 => Fractions.Monsters,
                _ => throw new ArgumentException("Invalid fraction")
            };

            try
            {
                var cards = await dbContext.Cards
                    .Where(c => c.fraction == fractionEnum)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        Fraction = (int)c.fraction,
                        Ability = (int)c.ability,
                        Strength = c.Strength,
                        Place = (int)c.place,
                        IsChampion = c.isChampion,
                        IsCommander = c.isCommander,
                        IsSpecial = c.isSpecial,
                    })
                    .ToListAsync();

                return Ok(cards);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetAvailableCards({fraction}): {ex}");
                return StatusCode(500, new
                {
                    error = "Błąd pobierania puli kart",
                    message = ex.Message,
                    stack = ex.StackTrace
                });
            }
        }
    }
}