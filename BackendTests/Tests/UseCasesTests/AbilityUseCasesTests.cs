using Backend.Models;
using Backend.Models.Enums;
using Backend.UseCases;
using Xunit;

namespace Backend.Tests.UseCasesTests
{
    public class AbilityUseCasesTests
    {
        private AbilityUseCases abilityUseCases;

        public AbilityUseCasesTests()
        {
            abilityUseCases = new AbilityUseCases();
        }

        private Game CreateBasicGame()
        {
            // Decki graczy
            var player1Deck = new List<Card>
            {
                new Card("CardA", Fractions.NorthernRealms, Abilities.brak, 5, Place.FirstRow, false, false, false),
                new Card("CardB", Fractions.NorthernRealms, Abilities.brak, 3, Place.SecondRow, false, false, false)
            };
            var player2Deck = new List<Card>
            {
                new Card("CardC", Fractions.Monsters, Abilities.brak, 4, Place.FirstRow, false, false, false),
                new Card("CardD", Fractions.Monsters, Abilities.brak, 2, Place.SecondRow, false, false, false)
            };

            var game = new Game(player1Deck, player2Deck, Fractions.NorthernRealms, Fractions.Monsters);

            game.Player1 = new Player("Player1", "hash1");
            game.Player2 = new Player("Player2", "hash2");
            game.CurrentPlayer = game.Player1;

            return game;
        }

        [Fact]
        public void BraterstwoAbility_ShouldMoveAllCardsWithSameNameToBoard()
        {
            var game = CreateBasicGame();

            var card1 = new Card("CardA", Fractions.NorthernRealms, Abilities.braterstwo, 5, Place.FirstRow, false, false, false);
            var card2 = new Card("CardA", Fractions.NorthernRealms, Abilities.braterstwo, 5, Place.FirstRow, false, false, false);
            game.Player1CardInDeck.Add(card1);
            game.Player1CardsOnHand.Add(card2);

            abilityUseCases.BraterstwoAbility(game, card1);

            Assert.Contains(card1, game.Board.Player1FirstCardRow);
            Assert.Contains(card2, game.Board.Player1FirstCardRow);
            Assert.DoesNotContain(card1, game.Player1CardInDeck);
            Assert.DoesNotContain(card2, game.Player1CardsOnHand);
        }

        [Fact]
        public void SzpiegAbility_ShouldAddCardToOpponentAndDrawTwo()
        {
            var game = CreateBasicGame();

            var spyCard = new Card("Spy", Fractions.NorthernRealms, Abilities.szpieg, 4, Place.FirstRow, false, false, false);
            game.Player1CardInDeck.AddRange(new[]
            {
                new Card("A", Fractions.NorthernRealms, Abilities.brak, 3, Place.FirstRow, false, false, false),
                new Card("B", Fractions.NorthernRealms, Abilities.brak, 2, Place.SecondRow, false, false, false)
            });

            abilityUseCases.SzpiegAbility(game, spyCard);

            Assert.Contains(spyCard, game.Board.Player2FirstCardRow);
            Assert.Equal(2, game.Player1CardsOnHand.Count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void ZwinnoscAbility_ShouldAddCardToSelectedRow(int row)
        {
            var game = CreateBasicGame();
            var agileCard = new Card("Agile", Fractions.NorthernRealms, Abilities.zwinnośc, 3, Place.AllRows, false, false, false);

            abilityUseCases.ZwinnoscAbility(game, agileCard, row);

            if (row == 1)
                Assert.Contains(agileCard, game.Board.Player1FirstCardRow);
            else if (row == 2)
                Assert.Contains(agileCard, game.Board.Player1SecondCardRow);
            else
                Assert.Contains(agileCard, game.Board.Player1ThirdCardRow);
        }

        [Fact]
        public void WskrzeszenieAbility_ShouldReviveCardToBoard()
        {
            var game = CreateBasicGame();
            var reviveCard = new Card("Revive", Fractions.NorthernRealms, Abilities.wskrzeszenie, 4, Place.FirstRow, false, false, false);
            game.Player1CardsOnDisplay.Add(reviveCard);

            abilityUseCases.WskrzeszenieAbility(game, reviveCard);

            Assert.Contains(reviveCard, game.Board.Player1FirstCardRow);
        }

        [Fact]
        public void PozogaJednostkiAbility_ShouldDestroyStrongestNonChampion()
        {
            var game = CreateBasicGame();
            var enemyCard1 = new Card("Enemy1", Fractions.Monsters, Abilities.brak, 5, Place.FirstRow, false, false, false);
            var enemyCard2 = new Card("Enemy2", Fractions.Monsters, Abilities.brak, 8, Place.FirstRow, false, false, false);
            var enemyCard3 = new Card("Boss", Fractions.Monsters, Abilities.brak, 7, Place.FirstRow, true, false, false);

            game.Board.Player2FirstCardRow.AddRange(new[] { enemyCard1, enemyCard2, enemyCard3 });
            game.Player2CardsOnDisplay.Clear();

            var playedCard = new Card("Pyro", Fractions.NorthernRealms, Abilities.pozogaJednostki, 3, Place.FirstRow, false, false, false);

            abilityUseCases.PozogaJednostkiAbility(game, playedCard);

            Assert.DoesNotContain(enemyCard2, game.Board.Player2FirstCardRow);
            Assert.Contains(enemyCard2, game.Player2CardsOnDisplay);
        }

        [Fact]
        public void WiezAbility_ShouldMultiplyStrengthForSameNameCards()
        {
            var game = CreateBasicGame();
            var card1 = new Card("Bond", Fractions.NorthernRealms, Abilities.wiez, 2, Place.FirstRow, false, false, false);
            var card2 = new Card("Bond", Fractions.NorthernRealms, Abilities.wiez, 3, Place.FirstRow, false, false, false);

            var row = new List<Card> { card1, card2 };
            abilityUseCases.WiezAbility(game, row);

            Assert.Equal(4, card1.finalStrength);
            Assert.Equal(6, card2.finalStrength);
        }

        [Fact]
        public void WyzszeMoraleAbility_ShouldIncreaseOtherCardsStrength()
        {
            var game = CreateBasicGame();
            var moraleCard = new Card("Morale", Fractions.NorthernRealms, Abilities.wyzszeMorale, 3, Place.FirstRow, false, false, false);
            var otherCard = new Card("Soldier", Fractions.NorthernRealms, Abilities.brak, 2, Place.FirstRow, false, false, false);

            var row = new List<Card> { moraleCard, otherCard };
            abilityUseCases.WyzszeMoraleAbility(game, row);

            Assert.Equal(3, moraleCard.finalStrength);
            Assert.Equal(3, otherCard.finalStrength);
        }

        [Fact]
        public void RogDowodcyJednostkiAbility_ShouldDoubleStrengthForNonChampionCards()
        {
            var game = CreateBasicGame();
            var hornCard = new Card("Horn", Fractions.NorthernRealms, Abilities.rogDowodcyJednostki, 3, Place.FirstRow, false, false, false);
            var otherCard = new Card("Soldier", Fractions.NorthernRealms, Abilities.brak, 2, Place.FirstRow, false, false, false);
            var row = new List<Card> { hornCard, otherCard };

            abilityUseCases.RogDowodcyJednostkiAbility(game, row);

            Assert.Equal(3, hornCard.finalStrength);
            Assert.Equal(4, otherCard.finalStrength);
        }

        [Fact]
        public void ZwinnoscAbility_ShouldNotAddCardIfRowNotValid()
        {
            var game = CreateBasicGame();
            var agileCard = new Card("Agile", Fractions.NorthernRealms, Abilities.zwinnośc, 3, Place.FirstRow, false, false, false);

            abilityUseCases.ZwinnoscAbility(game, agileCard, 2);

            Assert.DoesNotContain(agileCard, game.Board.Player1SecondCardRow);
            Assert.DoesNotContain(agileCard, game.Board.Player1ThirdCardRow);
        }

        [Fact]
        public void WskrzeszenieAbility_ShouldNotReviveCardNotInGraveyard()
        {
            var game = CreateBasicGame();
            var reviveCard = new Card("Revive", Fractions.NorthernRealms, Abilities.wskrzeszenie, 4, Place.FirstRow, false, false, false);

            abilityUseCases.WskrzeszenieAbility(game, reviveCard);

            Assert.Empty(game.Board.Player1FirstCardRow);
        }

        [Fact]
        public void PozogaJednostkiAbility_ShouldNotAffectChampion()
        {
            var game = CreateBasicGame();
            var championCard = new Card("Boss", Fractions.Monsters, Abilities.brak, 8, Place.FirstRow, true, false, false);
            game.Board.Player2FirstCardRow.Add(championCard);
            game.Player2CardsOnDisplay.Clear();

            var playedCard = new Card("Pyro", Fractions.NorthernRealms, Abilities.pozogaJednostki, 3, Place.FirstRow, false, false, false);

            abilityUseCases.PozogaJednostkiAbility(game, playedCard);

            Assert.Contains(championCard, game.Board.Player2FirstCardRow);
        }

        [Fact]
        public void SzpiegAbility_ShouldDrawOnlyIfDeckHasCards()
        {
            var game = CreateBasicGame();
            var spyCard = new Card("Spy", Fractions.NorthernRealms, Abilities.szpieg, 4, Place.FirstRow, false, false, false);

            game.Player1CardInDeck.Clear();

            abilityUseCases.SzpiegAbility(game, spyCard);

            Assert.Empty(game.Player1CardsOnHand);
        }

        [Fact]
        public void BraterstwoAbility_ShouldHandleNoMatchingCards()
        {
            var game = CreateBasicGame();
            var playedCard = new Card("NonExisting", Fractions.NorthernRealms, Abilities.braterstwo, 3, Place.FirstRow, false, false, false);

            abilityUseCases.BraterstwoAbility(game, playedCard);

            Assert.Empty(game.Board.Player1FirstCardRow);
            Assert.Empty(game.Board.Player1SecondCardRow);
            Assert.Empty(game.Board.Player1ThirdCardRow);
        }

    }
}