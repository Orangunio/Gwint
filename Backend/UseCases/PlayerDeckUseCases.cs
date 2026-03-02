using Backend.Models;

namespace Backend.UseCases
{
    public class PlayerDeckUseCases
    {
        public PlayerDeck AddCardToCollection(int playerId, int cardId)
        {
            // Tworzymy powiązanie między graczem a kartą
            return new PlayerDeck(playerId, cardId);
        }
        public List<Card> GetCardsForGame(List<PlayerDeck> playerDecks)
        {
            return playerDecks
                .Where(pd => pd.Card != null)
                .Select(pd => pd.Card)
                .ToList();
        }
        public bool RemoveCardFromCollection(List<PlayerDeck> playerDecks, int cardId)
        {
            var itemToRemove = playerDecks.FirstOrDefault(pd => pd.CardId == cardId);
            if (itemToRemove != null)
            {
                playerDecks.Remove(itemToRemove);
                return true;
            }
            return false;
        }
    }
}
