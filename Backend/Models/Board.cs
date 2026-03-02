using System.ComponentModel.DataAnnotations.Schema;

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
        [NotMapped]
        public int[,] RowScores { get; set; }

        //Jeszcze powinno być coś do sprawdzania efektów kart, ale to później

        public Board()
        {
            Player1FirstCardRow = new List<Card>();
            Player1SecondCardRow = new List<Card>();
            Player1ThirdCardRow = new List<Card>();
            Player2FirstCardRow = new List<Card>();
            Player2SecondCardRow = new List<Card>();
            Player2ThirdCardRow = new List<Card>();
            RowScores = new int[2, 3];
        }
    }
}
