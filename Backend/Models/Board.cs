namespace Backend.Models
{
    public class Board
    {
        public List<Card> Player1FirstCardRow { get; set; }
        public List<Card> Player1SecondCardRow { get; set; }
        public List<Card> Player1ThirdCardRow { get; set; }

        public List<Card> Player2FirstCardRow { get; set; }
        public List<Card> Player2SecondCardRow { get; set; }
        public List<Card> Player2ThirdCardRow { get; set; }

        public int player1FirstRowScore { get; set; }
        public int player1SecondRowScore { get; set; }
        public int player1ThirdRowScore { get; set; }

        public int player2FirstRowScore { get; set; }
        public int player2SecondRowScore { get; set; }
        public int player2ThirdRowScore { get; set; }

        //Jeszcze powinno być coś do sprawdzania efektów kart, ale to później

        public Board()
        {
            Player1FirstCardRow = new List<Card>();
            Player1SecondCardRow = new List<Card>();
            Player1ThirdCardRow = new List<Card>();
            Player2FirstCardRow = new List<Card>();
            Player2SecondCardRow = new List<Card>();
            Player2ThirdCardRow = new List<Card>();
            player1FirstRowScore = 0;
            player1SecondRowScore = 0;
            player1ThirdRowScore = 0;
            player2FirstRowScore = 0;
            player2SecondRowScore = 0;
            player2ThirdRowScore = 0;
        }
    }
}
