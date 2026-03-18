using Backend.Models;
using Backend.Models.Enums;

namespace Backend.UseCases
{
    public class AbilityUseCases
    {
        public AbilityUseCases()
        {

        }

        #region Helper

        public void AddCardToBoard(Game game, Player player, Place place, Card card)
        {
            bool isPlayer1 = player == game.Player1;

            if (place == Place.FirstRow)
            {
                if (isPlayer1)
                    game.Board.Player1FirstCardRow.Add(card);
                else
                    game.Board.Player2FirstCardRow.Add(card);
            }
            else if (place == Place.SecondRow)
            {
                if (isPlayer1)
                    game.Board.Player1SecondCardRow.Add(card);
                else
                    game.Board.Player2SecondCardRow.Add(card);
            }
            else if (place == Place.ThirdRow)
            {
                if (isPlayer1)
                    game.Board.Player1ThirdCardRow.Add(card);
                else
                    game.Board.Player2ThirdCardRow.Add(card);
            }
        }

        #endregion


        #region Umiejętności na kartach jednostek

        public void BraterstwoAbility(Game game, Card playedCard)
        {
            var currentPlayer = game.CurrentPlayer;

            List<Card> playerDeck;
            List<Card> playerHand;

            if (currentPlayer == game.Player1)
            {
                playerDeck = game.Player1CardInDeck;
                playerHand = game.Player1CardsOnHand;
            }
            else
            {
                playerDeck = game.Player2CardInDeck;
                playerHand = game.Player2CardsOnHand;
            }

            List<Card> cards = new List<Card>();

            // karty z talii
            var deckCards = playerDeck.Where(c => c.Name == playedCard.Name).ToList();
            foreach (var c in deckCards)
            {
                cards.Add(c);
                playerDeck.Remove(c);
            }

            // karty z ręki
            var handCards = playerHand.Where(c => c.Name == playedCard.Name).ToList();
            foreach (var c in handCards)
            {
                cards.Add(c);
                playerHand.Remove(c);
            }

            // dodanie kart na planszę
            foreach (var c in cards)
            {
                AddCardToBoard(game, currentPlayer, c.place, c);
            }
        }

        public void SzpiegAbility(Game game, Card playedCard)
        {
            var currentPlayer = game.CurrentPlayer;

            Player opponent = currentPlayer == game.Player1
                ? game.Player2
                : game.Player1;

            // karta trafia do przeciwnika
            AddCardToBoard(game, opponent, playedCard.place, playedCard);

            // dobranie 2 kart
            List<Card> playerDeck;
            List<Card> playerHand;

            if (currentPlayer == game.Player1)
            {
                playerDeck = game.Player1CardInDeck;
                playerHand = game.Player1CardsOnHand;
            }
            else
            {
                playerDeck = game.Player2CardInDeck;
                playerHand = game.Player2CardsOnHand;
            }

            for (int i = 0; i < 2; i++)
            {
                if (playerDeck.Count > 0)
                {
                    var card = playerDeck[0];
                    playerDeck.RemoveAt(0);
                    playerHand.Add(card);
                }
            }
        }

        public void ZwinnoscAbility(Game game, Card playedCard, int selectedRow)
        {
            var currentPlayer = game.CurrentPlayer;

            List<Card> firstRow;
            List<Card> secondRow;
            List<Card> thirdRow;

            if (currentPlayer == game.Player1)
            {
                firstRow = game.Board.Player1FirstCardRow;
                secondRow = game.Board.Player1SecondCardRow;
                thirdRow = game.Board.Player1ThirdCardRow;
            }
            else
            {
                firstRow = game.Board.Player2FirstCardRow;
                secondRow = game.Board.Player2SecondCardRow;
                thirdRow = game.Board.Player2ThirdCardRow;
            }


            if (selectedRow == 1 && (playedCard.place == Place.FirstRow || playedCard.place == Place.FirstAndSecondRow || playedCard.place == Place.FirstAndThirdRow || 
                playedCard.place == Place.AllRows))
            {
                firstRow.Add(playedCard);
            }
            else if (selectedRow == 2 && (playedCard.place == Place.SecondRow || playedCard.place == Place.FirstAndSecondRow || playedCard.place == Place.SecondAndThirdRow ||
                playedCard.place == Place.AllRows))
            {
                secondRow.Add(playedCard);
            }
            else if (selectedRow == 3 && (playedCard.place == Place.ThirdRow || playedCard.place == Place.FirstAndThirdRow || playedCard.place == Place.SecondAndThirdRow ||
                playedCard.place == Place.AllRows))
            {
                thirdRow.Add(playedCard);
            }
        }

        //Trzeba będzie dodać rozpatrywanie umiejętności karty po wskrzeszeniu
        public void WskrzeszenieAbility(Game game, Card reviveCard)
        {
            var currentPlayer = game.CurrentPlayer;
            List<Card> playerGraveyard;
            List<Card> firstRow;
            List<Card> secondRow;
            List<Card> thirdRow;

            if (currentPlayer == game.Player1)
            {
                playerGraveyard = game.Player1CardsOnDisplay;
                firstRow = game.Board.Player1FirstCardRow;
                secondRow = game.Board.Player1SecondCardRow;
                thirdRow = game.Board.Player1ThirdCardRow;
            }
            else
            {
                playerGraveyard = game.Player2CardsOnDisplay;
                firstRow = game.Board.Player2FirstCardRow;
                secondRow = game.Board.Player2SecondCardRow;
                thirdRow = game.Board.Player2ThirdCardRow;
            }

            if (reviveCard != null)
            {
                if (!playerGraveyard.Contains(reviveCard))
                    return;

                if (reviveCard.place == Place.FirstRow || reviveCard.place == Place.FirstAndSecondRow || reviveCard.place == Place.FirstAndThirdRow ||
                    reviveCard.place == Place.AllRows)
                {
                    firstRow.Add(reviveCard);
                }
                else if (reviveCard.place == Place.SecondRow || reviveCard.place == Place.FirstAndSecondRow || reviveCard.place == Place.SecondAndThirdRow ||
                    reviveCard.place == Place.AllRows)
                {
                    secondRow.Add(reviveCard);
                }
                else if (reviveCard.place == Place.ThirdRow || reviveCard.place == Place.FirstAndThirdRow || reviveCard.place == Place.SecondAndThirdRow ||
                    reviveCard.place == Place.AllRows)
                {
                    thirdRow.Add(reviveCard);
                }
            }
        }

        public void WiezAbility(Game game)
        {
            game.Board.CalculateRowScores();
        }

        public void WyzszeMoraleAbility(Game game)
        {
            game.Board.CalculateRowScores();
        }

        public void PozogaJednostkiAbility(Game game, Card playedCard)
        {
            var currentPlayer = game.CurrentPlayer;

            List<Card> enemyRow = new();
            List<Card> enemyGraveyard;
            bool weatherEffect = false;
            bool hornEffect = false;

            if (currentPlayer == game.Player1)
            {
                enemyGraveyard = game.Player2CardsOnDisplay;

                if (playedCard.place == Place.FirstRow)
                {
                    enemyRow = game.Board.Player2FirstCardRow;
                    weatherEffect = game.Board.FrostActive;
                    hornEffect = game.Board.RogDowodcyActive[1, 0];
                }
                else if (playedCard.place == Place.SecondRow)
                {
                    enemyRow = game.Board.Player2SecondCardRow;
                    weatherEffect = game.Board.FogActive;
                    hornEffect = game.Board.RogDowodcyActive[1, 1];
                }
                else if (playedCard.place == Place.ThirdRow)
                {
                    enemyRow = game.Board.Player2ThirdCardRow;
                    weatherEffect = game.Board.FogActive;
                    hornEffect = game.Board.RogDowodcyActive[1, 2];
                }
            }
            else
            {
                enemyGraveyard = game.Player1CardsOnDisplay;

                if (playedCard.place == Place.FirstRow)
                {
                    enemyRow = game.Board.Player1FirstCardRow;
                    weatherEffect = game.Board.FrostActive;
                    hornEffect = game.Board.RogDowodcyActive[0, 0];
                }
                else if (playedCard.place == Place.SecondRow)
                {
                    enemyRow = game.Board.Player1SecondCardRow;
                    weatherEffect = game.Board.FogActive;
                    hornEffect = game.Board.RogDowodcyActive[0, 1];
                }
                else if (playedCard.place == Place.ThirdRow)
                {
                    enemyRow = game.Board.Player1ThirdCardRow;
                    weatherEffect = game.Board.FogActive;
                    hornEffect = game.Board.RogDowodcyActive[0, 2];
                }
            }

            // przelicz rząd
            game.Board.CalculateRow(enemyRow, weatherEffect, hornEffect);

            int strengthSum = enemyRow.Sum(c => c.finalStrength);

            if (strengthSum >= 10)
            {
                int maxStrength = enemyRow
                    .Where(c => !c.isChampion)
                    .Max(c => c.finalStrength);

                var cardsToDestroy = enemyRow
                    .Where(c => !c.isChampion && c.finalStrength == maxStrength)
                    .ToList();

                foreach (var card in cardsToDestroy)
                {
                    enemyRow.Remove(card);
                    enemyGraveyard.Add(card);
                }
            }
        }

        public void RogDowodcyJednostkiAbility(Game game)
        {
            game.Board.CalculateRowScores();
        }

        public void BydleceSilyZbrojneAbility()
        {
            //Dodać trzeba BSZ do pierwszego rzędu
        }
        #endregion
    }
}