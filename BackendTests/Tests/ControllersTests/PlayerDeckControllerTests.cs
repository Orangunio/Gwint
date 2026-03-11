using Backend.Controllers;
using Backend.Database;
using Backend.Models;
using Backend.Models.Body;
using Backend.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Backend.Tests.ControllersTests
{
    public class PlayerDeckControllerTests
    {
        private GwintDBContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<GwintDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDB")
                .Options;
            var context = new GwintDBContext(options);

            if (!context.Cards.Any())
            {
                context.Cards.AddRange(
                    new Card("Card1", Fractions.Nilfgaard, Abilities.brak, 5, Place.FirstRow, false, true, false),
                    new Card("Card2", Fractions.Nilfgaard, Abilities.brak, 3, Place.FirstRow, false, false, false),
                    new Card("Card3", Fractions.NorthernRealms, Abilities.brak, 4, Place.SecondRow, false, false, true),
                    new Card("Card4", Fractions.NorthernRealms, Abilities.brak, 2, Place.ThirdRow, false, false, false),
                    new Card("Card5", Fractions.ScoiaTael, Abilities.brak, 6, Place.FirstRow, false, false, true)
                );
                context.SaveChanges();
            }

            if (!context.PlayerDecks.Any())
            {
                context.PlayerDecks.AddRange(
                    new PlayerDeck { Id = 1, PlayerId = 1, CardId = 1 },
                    new PlayerDeck { Id = 2, PlayerId = 1, CardId = 2 },
                    new PlayerDeck { Id = 3, PlayerId = 2, CardId = 3 }
                );
                context.SaveChanges();
            }

            return context;
        }

        [Fact]
        public async Task GetPlayerDecks_ReturnsAllPlayerDecks()
        {
            var db = GetDbContext();
            var controller = new PlayerDeckController(db);

            var result = await controller.GetPlayerDecks(1) as OkObjectResult;

            Assert.NotNull(result);
            var decks = result.Value as IEnumerable<object>;
            Assert.Equal(2, decks.Count());
        }

        [Fact]
        public async Task GetPlayerFractionDecks_ReturnsFilteredByFraction()
        {
            var db = GetDbContext();
            var controller = new PlayerDeckController(db);

            var result = await controller.GetPlayerDecks(1, 1) as OkObjectResult;

            Assert.NotNull(result);
            var decks = result.Value as IEnumerable<object>;

            Assert.Equal(2, decks.Count());
        }

        [Fact]
        public async Task GetPlayerFractionDecks_InvalidFraction_ReturnsBadRequest()
        {
            var db = GetDbContext();
            var controller = new PlayerDeckController(db);

            var result = await controller.GetPlayerDecks(1, 99);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateDeck_TooSmallCardAmount_ReturnsBadRequest()
        {
            var db = GetDbContext();
            var controller = new PlayerDeckController(db);

            var body = new UpdateDeckBody
            {
                PlayerId = 1,
                Fraction = 1,
                CardIdsToAdd = new List<int>(),
                CardIdsToRemove = new List<int> { 1, 2, 3 }
            };

            var result = await controller.UpdateDeck(body);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateDeck_MultipleCommanders_ReturnsBadRequest()
        {
            var db = GetDbContext();
            var controller = new PlayerDeckController(db);

            db.Cards.Add(new Card("Commander2", Fractions.Nilfgaard, Abilities.brak, 7, Place.WithoutRow, false, true, false));
            db.SaveChanges();

            var body = new UpdateDeckBody
            {
                PlayerId = 1,
                Fraction = 1,
                CardIdsToAdd = new List<int> { db.Cards.Last().Id },
                CardIdsToRemove = new List<int>()
            };

            var result = await controller.UpdateDeck(body);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateDeck_TooManySpecialCards_ReturnsBadRequest()
        {
            var db = GetDbContext();
            var controller = new PlayerDeckController(db);

            for (int i = 10; i <= 20; i++)
            {
                db.Cards.Add(new Card($"Special{i}", Fractions.NorthernRealms, Abilities.manekinDoCwiczen, 2, Place.AllRows, false, false, true));
            }
            db.SaveChanges();

            var body = new UpdateDeckBody
            {
                PlayerId = 2,
                Fraction = 2,
                CardIdsToAdd = Enumerable.Range(10, 11).ToList(),
                CardIdsToRemove = new List<int>()
            };

            var result = await controller.UpdateDeck(body);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateDeck_TooFewCards_ReturnsBadRequest()
        {
            var db = GetDbContext();
            var controller = new PlayerDeckController(db);

            var body = new UpdateDeckBody
            {
                PlayerId = 1,
                Fraction = 1,
                CardIdsToAdd = new List<int>(),
                CardIdsToRemove = new List<int> { 2 }
            };

            var result = await controller.UpdateDeck(body);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateDeck_InvalidFraction_ReturnsBadRequest()
        {
            var db = GetDbContext();
            var controller = new PlayerDeckController(db);

            var body = new UpdateDeckBody
            {
                PlayerId = 1,
                Fraction = 99,
                CardIdsToAdd = new List<int>(),
                CardIdsToRemove = new List<int>()
            };

            var result = await controller.UpdateDeck(body);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateDeck_AddExistingCard_DoesNotDuplicate_WithCommander()
        {
            var options = new DbContextOptionsBuilder<GwintDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var db = new GwintDBContext(options);
            var controller = new PlayerDeckController(db);

            for (int i = 1; i <= 29; i++)
            {
                var card = new Card($"Card{i}", Fractions.Nilfgaard, Abilities.brak, 5, Place.FirstRow, false, false, false);
                db.Cards.Add(card);
                db.SaveChanges();

                db.PlayerDecks.Add(new PlayerDeck { PlayerId = 1, CardId = card.Id });
            }

            var commanderCard = new Card("Commander", Fractions.Nilfgaard, Abilities.brak, 10, Place.FirstRow, true, true, false);
            db.Cards.Add(commanderCard);
            db.SaveChanges();

            db.PlayerDecks.Add(new PlayerDeck { PlayerId = 1, CardId = commanderCard.Id });
            db.SaveChanges();

            var existingCardId = db.PlayerDecks.First(pd => pd.PlayerId == 1).CardId;

            var body = new UpdateDeckBody
            {
                PlayerId = 1,
                Fraction = 1,
                CardIdsToAdd = new List<int> { existingCardId },
                CardIdsToRemove = new List<int>()
            };

            await controller.UpdateDeck(body);

            var updatedDeck = await db.PlayerDecks
                .Where(pd => pd.PlayerId == 1)
                .Include(pd => pd.Card)
                .ToListAsync();

            Assert.Equal(30, updatedDeck.Count());
            Assert.Contains(updatedDeck, d => d.Card.isCommander);

            var groupedByCardId = updatedDeck.GroupBy(d => d.CardId);
            Assert.All(groupedByCardId, g => Assert.Single(g));
        }

        [Fact]
        public async Task GetPlayerDecks_NoDeck_ReturnsEmptyList()
        {
            var db = GetDbContext();
            var controller = new PlayerDeckController(db);

            var result = await controller.GetPlayerDecks(99) as OkObjectResult;
            var decks = result.Value as IEnumerable<object>;

            Assert.Empty(decks);
        }
    }
}
