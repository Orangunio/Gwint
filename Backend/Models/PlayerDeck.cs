namespace Backend.Models
{
    public class PlayerDeck
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; }
        public int CardId { get; set; }
        public Card Card { get; set; }

        public PlayerDeck() { }
        public PlayerDeck(int playerId, int cardId)
        {
            PlayerId = playerId;
            CardId = cardId;
        }
    }
}