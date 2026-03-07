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

        private void AddCardToBoard(Game game, Player player, Place place, Card card)
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


        #endregion
    }
}