using Backend.Models;
using Backend.Models.Enums;
using System.ComponentModel;

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
                playerDeck = game.Player1CardsInDeck;
                playerHand = game.Player1CardsOnHand;
            }
            else
            {
                playerDeck = game.Player2CardsInDeck;
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
                playerDeck = game.Player1CardsInDeck;
                playerHand = game.Player1CardsOnHand;
            }
            else
            {
                playerDeck = game.Player2CardsInDeck;
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
                    hornEffect = game.Board.RogDowodcyActive[1][0];
                }
                else if (playedCard.place == Place.SecondRow)
                {
                    enemyRow = game.Board.Player2SecondCardRow;
                    weatherEffect = game.Board.FogActive;
                    hornEffect = game.Board.RogDowodcyActive[1][1];
                }
                else if (playedCard.place == Place.ThirdRow)
                {
                    enemyRow = game.Board.Player2ThirdCardRow;
                    weatherEffect = game.Board.FogActive;
                    hornEffect = game.Board.RogDowodcyActive[1][2];
                }
            }
            else
            {
                enemyGraveyard = game.Player1CardsOnDisplay;

                if (playedCard.place == Place.FirstRow)
                {
                    enemyRow = game.Board.Player1FirstCardRow;
                    weatherEffect = game.Board.FrostActive;
                    hornEffect = game.Board.RogDowodcyActive[0][0];
                }
                else if (playedCard.place == Place.SecondRow)
                {
                    enemyRow = game.Board.Player1SecondCardRow;
                    weatherEffect = game.Board.FogActive;
                    hornEffect = game.Board.RogDowodcyActive[0][1];
                }
                else if (playedCard.place == Place.ThirdRow)
                {
                    enemyRow = game.Board.Player1ThirdCardRow;
                    weatherEffect = game.Board.FogActive;
                    hornEffect = game.Board.RogDowodcyActive[0][2];
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
                    card.finalStrength = card.Strength;
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

        #region Umiejetnosci kart specjalnych

        public void RogDowodcy(Game game, int row)
        {
            if(game.CurrentPlayer == game.Player1)
            {
                switch (row)
                {
                    case 1:
                        game.Board.RogDowodcyActive[0][0] = true;
                        break;
                    case 2:
                        game.Board.RogDowodcyActive[0][1] = true;
                        break;
                    case 3:
                        game.Board.RogDowodcyActive[0][2] = true;
                        break;
                }
            }
            else
            {
                switch (row)
                {
                    case 1:
                        game.Board.RogDowodcyActive[1][0] = true;
                        break;
                    case 2:
                        game.Board.RogDowodcyActive[1][1] = true;
                        break;
                    case 3:
                        game.Board.RogDowodcyActive[1][2] = true;
                        break;
                }
            }

             game.Board.CalculateAllRows(game);
        }

        public void Pozoga(Game game)
        {
            int maxStrength = 0;

            var allCards = game.Board.Player1FirstCardRow
            .Concat(game.Board.Player1SecondCardRow)
            .Concat(game.Board.Player1ThirdCardRow)
            .Concat(game.Board.Player2FirstCardRow)
            .Concat(game.Board.Player2SecondCardRow)
            .Concat(game.Board.Player2ThirdCardRow)
            .ToList();

            if (allCards.Any())
            {
                maxStrength = allCards
                    .Where(c => !c.isChampion)
                    .Max(c => c.finalStrength);
            }

            void RemoveStrongest(List<Card> row, List<Card> graveyard)
            {
                var toRemove = row
                    .Where(c => c.finalStrength == maxStrength && !c.isChampion)
                    .ToList();

                foreach (var card in toRemove)
                {
                    card.finalStrength = card.Strength;
                    row.Remove(card);
                    graveyard.Add(card);
                }
            }

            // Player 1
            RemoveStrongest(game.Board.Player1FirstCardRow, game.Player1CardsOnDisplay);
            RemoveStrongest(game.Board.Player1SecondCardRow, game.Player1CardsOnDisplay);
            RemoveStrongest(game.Board.Player1ThirdCardRow, game.Player1CardsOnDisplay);

            // Player 2
            RemoveStrongest(game.Board.Player2FirstCardRow, game.Player2CardsOnDisplay);
            RemoveStrongest(game.Board.Player2SecondCardRow, game.Player2CardsOnDisplay);
            RemoveStrongest(game.Board.Player2ThirdCardRow, game.Player2CardsOnDisplay);
        }

        public void ManekinDoCwiczenAbility(Game game, Card selectedCard)
        {
            var currentPlayer = game.CurrentPlayer;

            List<Card> playerHand;

            List<List<Card>> playerRows;

            if (currentPlayer == game.Player1)
            {
                playerHand = game.Player1CardsOnHand;

                playerRows = new List<List<Card>>
                {
                    game.Board.Player1FirstCardRow,
                    game.Board.Player1SecondCardRow,
                    game.Board.Player1ThirdCardRow
                 };
            }
            else
            {
                playerHand = game.Player2CardsOnHand;

                playerRows = new List<List<Card>>
                {
                    game.Board.Player2FirstCardRow,
                    game.Board.Player2SecondCardRow,
                    game.Board.Player2ThirdCardRow
                };
            }

            // znajdź kartę na planszy
            foreach (var row in playerRows)
            {
                if (row.Contains(selectedCard))
                {
                    row.Remove(selectedCard);
                    playerHand.Add(selectedCard);
                    break;
                }
            }
        }

        public void TrzaskajacyMrozAbility(Game game)
        {
            game.Board.FrostActive = true;
            game.Board.CalculateRowScores();
        }

        public void GestaMglaAbility(Game game)
        {
            game.Board.FogActive = true;
            game.Board.CalculateRowScores();
        }

        public void UlewnyDeszczAbility(Game game)
        {
            game.Board.RainActive = true;
            game.Board.CalculateRowScores();
        }

        public void CzysteNieboAbility(Game game)
        {
            game.Board.FrostActive = false;
            game.Board.FogActive = false;
            game.Board.RainActive = false;

            game.Board.CalculateRowScores();
        }
        #endregion

        #region Dowodcy

        //Nilfgard
        public List<Card> EmperorOfNilfgardAbility(List<Card> enemyHand)
        {
            Random rnd = new Random();

            int count = Math.Min(3, enemyHand.Count);

            return enemyHand
                .OrderBy(x => rnd.Next())
                .Take(count)
                .ToList();
        }

        public void HisEmperialmajestyAbility(Game game)
        {
            game.Board.RainActive = true;
            game.Board.CalculateRowScores();
        }

        public void InvaiderOfTheNorthAbility(Game game, Card card)
        {
            Random rnd = new Random();

            int row = rnd.Next(0, 3);

            Place place;

            if(row == 0)
            {
                place = Place.FirstRow;
            }
            else if (row == 1)
            {
                place = Place.SecondRow;
            }
            else
            {
                place = Place.ThirdRow;
            }

            AddCardToBoard(game, game.CurrentPlayer ,place, card);
        }

        public void TheRelentlessAbility(Game game, Card card)
        {
            if(game.CurrentPlayer == game.Player1) 
            { 
                game.Player1CardsOnHand.Add(card);
            }
            else
            {
                game.Player2CardsOnHand.Add(card);
            }
        }

        public void TheWhiteFlameAbility(Game game)
        {
            game.Player1CommanderCard = null;
            game.Player2CommanderCard = null;
        }


        // Polnoc
        public void KingOfTemeriaAbility(Game game)
        {
            if(game.CurrentPlayer == game.Player1)
            {
                var fogCard = game.Player1CardsInDeck.FirstOrDefault(c => c.ability == Abilities.gestaMgla);
                if(fogCard != null)
                {
                    game.Board.FogActive = true;
                }
                game.Player1CardsInDeck.Remove(fogCard);
            }
            else
            {
                var fogCard = game.Player2CardsInDeck.FirstOrDefault(c => c.ability == Abilities.gestaMgla);
                if (fogCard != null)
                {
                    game.Board.FogActive = true;
                }
                game.Player2CardsInDeck.Remove(fogCard);
            }
        }

        public void CommanderOfTheNorthAbility(Game game)
        {
            game.Board.FrostActive = false;
            game.Board.FogActive = false;
            game.Board.RainActive = false;
            game.Board.CalculateRowScores();
        }

        public void SonOfMedellAbility(Game game)
        {
            int rowStrength = 0;
            if(game.CurrentPlayer == game.Player1)
            {
                foreach(var c in game.Board.Player2SecondCardRow)
                {
                    rowStrength += c.finalStrength;
                }
                if (rowStrength < 10)
                {
                    return;
                }
                int maxStrength = game.Board.Player2SecondCardRow.Max(x => x.finalStrength);

                List<Card> cardsToDestroy = game.Board.Player2SecondCardRow.Where(x => x.finalStrength == maxStrength).ToList();

                foreach (var c in cardsToDestroy)
                {
                    game.Board.Player2SecondCardRow.Remove(c);
                }
            }
            else
            {
                foreach (var c in game.Board.Player1SecondCardRow)
                {
                    rowStrength += c.finalStrength;
                }
                if (rowStrength < 10)
                {
                    return;
                }
                int maxStrength = game.Board.Player1SecondCardRow.Max(x => x.finalStrength);

                List<Card> cardsToDestroy = game.Board.Player1SecondCardRow.Where(x => x.finalStrength == maxStrength).ToList();

                foreach (var c in cardsToDestroy)
                {
                    game.Board.Player1SecondCardRow.Remove(c);
                }
            }
        }

        public void TheSiegemasterAbility(Game game)
        {
            if(game.CurrentPlayer == game.Player1)
            {
                game.Board.RogDowodcyActive[0][2] = true;
                game.Board.CalculateRowScores();
            }
            else
            {
                game.Board.RogDowodcyActive[1][2] = true;
                game.Board.CalculateRowScores();
            }
        }

        public void TheSteelForged(Game game)
        {
            int rowStrength = 0;
            if (game.CurrentPlayer == game.Player1)
            {
                foreach (var c in game.Board.Player2ThirdCardRow)
                {
                    rowStrength += c.finalStrength;
                }
                if (rowStrength < 10)
                {
                    return;
                }
                int maxStrength = game.Board.Player2ThirdCardRow.Max(x => x.finalStrength);

                List<Card> cardsToDestroy = game.Board.Player2ThirdCardRow.Where(x => x.finalStrength == maxStrength).ToList();

                foreach (var c in cardsToDestroy)
                {
                    game.Board.Player2ThirdCardRow.Remove(c);
                }
            }
            else
            {
                foreach (var c in game.Board.Player1ThirdCardRow)
                {
                    rowStrength += c.finalStrength;
                }
                if (rowStrength < 10)
                {
                    return;
                }
                int maxStrength = game.Board.Player1ThirdCardRow.Max(x => x.finalStrength);

                List<Card> cardsToDestroy = game.Board.Player1ThirdCardRow.Where(x => x.finalStrength == maxStrength).ToList();

                foreach (var c in cardsToDestroy)
                {
                    game.Board.Player1ThirdCardRow.Remove(c);
                }
            }
        }
        #endregion
    }
}