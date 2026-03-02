using Backend.Models;

namespace Backend.UseCases
{
    public class GameUseCases
    {
        private readonly Random _random = new Random();
        private readonly BoardUseCases _boardUseCases = new BoardUseCases();
        private readonly CardUseCases _cardUseCases = new CardUseCases();
        public void InitializeGame(Game game)
        {
            ShuffleDeck(game.Player1CardInDeck);
            ShuffleDeck(game.Player2CardInDeck);
            DrawCards(game, 1, 10);
            DrawCards(game, 2, 10);
        }
        public void DrawCards(Game game, int playerNumber, int count)
        {
            var deck = playerNumber == 1 ? game.Player1CardInDeck : game.Player2CardInDeck;
            var hand = playerNumber == 1 ? game.Player1CardsOnHand : game.Player2CardsOnHand;

            for (int i = 0; i < count && deck.Count > 0; i++)
            {
                hand.Add(deck[0]);
                deck.RemoveAt(0);
            }
        }
        public void ShuffleDeck(List<Card> deck)
        {
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                (deck[k], deck[n]) = (deck[n], deck[k]);
            }
        }
        public bool PlayCard(Game game, int playerNum, Card card, int rowIdx)
        {
            int pIdx = playerNum - 1;
            var hand = playerNum == 1 ? game.Player1CardsOnHand : game.Player2CardsOnHand;

            if (!hand.Contains(card) || !_cardUseCases.CanBePlacedInRow(card, rowIdx))
                return false;

            hand.Remove(card);

            int strength = _cardUseCases.GetEffectiveStrength(card);
            _boardUseCases.AddPoints(game.Board, pIdx, rowIdx, strength);

            game.Player1Score = _boardUseCases.GetPlayerTotal(game.Board, 0);
            game.Player2Score = _boardUseCases.GetPlayerTotal(game.Board, 1);

            return true;
        }
    }
}
