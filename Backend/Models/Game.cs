namespace Backend.Models
{
    public class Game
    {
        public int Id { get; set; }
        public Player Player1 { get; set; }
        public List<Card> Player1CardsOnHand { get; set; }
        public List<Card> Player1CardsOnDisplay { get; set; }
        public List<Card> Player1CardInDeck { get; set; }
        public Card Player1CommanderCard { get; set; }
        public int Player1Score { get; set; }

        public Player Player2 { get; set; }
        public List<Card> Player2CardsOnHand { get; set; }
        public List<Card> Player2CardsOnDisplay { get; set; }
        public List<Card> Player2CardInDeck { get; set; }
        public Card Player2CommanderCard { get; set; }
        public int Player2Score { get; set; }

        public Board Board { get; set; }

        public Game(List<Card> player1Deck, List<Card> player2Deck)
        {
            Player1CardsOnHand = new List<Card>();
            Player1CardsOnDisplay = new List<Card>();
            Player1CardInDeck = player1Deck;
            Player2CardsOnHand = new List<Card>();
            Player2CardsOnDisplay = new List<Card>();
            Player2CardInDeck = player2Deck;
            Board = new Board();
        }
    }
}