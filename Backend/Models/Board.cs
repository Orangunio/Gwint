using Backend.Models.Enums;
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
        //Wiersz 0 dla gracza 1, wiersz 1 dla gracza 2
        public int[,] RowScores { get; set; }
        public bool[,] RogDowodcyActive { get; set; }

        //Obecna pogoda na planszy, która wpływa na siłę kart
        public bool FrostActive { get; set; }
        public bool FogActive { get; set; }
        public bool RainActive { get; set; }

        public Board()
        {
            Player1FirstCardRow = new List<Card>();
            Player1SecondCardRow = new List<Card>();
            Player1ThirdCardRow = new List<Card>();
            Player2FirstCardRow = new List<Card>();
            Player2SecondCardRow = new List<Card>();
            Player2ThirdCardRow = new List<Card>();
            RowScores = new int[2, 3];
            FrostActive = false;
            FogActive = false;
            RainActive = false;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    RogDowodcyActive[i, j] = false;
                }
            }
        }

        public void CalculateRowScores()
        {
            // Obliczanie siły kart na każdej linii dla obu graczy
            foreach (var card in Player1FirstCardRow)
                RowScores[0, 0] += card.finalStrength;
            foreach (var card in Player1SecondCardRow)
                RowScores[0, 1] += card.finalStrength;
            foreach (var card in Player1ThirdCardRow)
                RowScores[0, 2] += card.finalStrength;

            foreach (var card in Player2FirstCardRow)
                RowScores[1, 0] += card.finalStrength;
            foreach (var card in Player2SecondCardRow)
                RowScores[1, 1] += card.finalStrength;
            foreach (var card in Player2ThirdCardRow)
                RowScores[1, 2] += card.finalStrength;
        }

        public int GetPlayerTotalScore(bool currentPlayerIs1)
        {
            int totalScore = 0;

            if (currentPlayerIs1)
            {
                foreach(var card in Player1FirstCardRow)
                    totalScore += card.finalStrength;
                foreach (var card in Player1SecondCardRow)
                    totalScore += card.finalStrength;
                foreach (var card in Player1ThirdCardRow)
                    totalScore += card.finalStrength;
            }
            else
            {
                foreach (var card in Player2FirstCardRow)
                    totalScore += card.finalStrength;
                foreach (var card in Player2SecondCardRow)
                    totalScore += card.finalStrength;
                foreach (var card in Player2ThirdCardRow)
                    totalScore += card.finalStrength;
            }

            return totalScore;
        }

        public void CalculateRow(List<Card> row, bool frost, bool horn)
        {
            if (row == null || row.Count == 0)
                return;

            // 1. Reset siły
            foreach (var card in row)
            {
                card.finalStrength = card.Strength;
            }

            // 2. Pogoda
            if (frost)
            {
                foreach (var card in row)
                {
                    if (!card.isChampion)
                        card.finalStrength = 1;
                }
            }

            // 3. Wysokie morale
            foreach (var moraleCard in row.Where(c => c.ability == Abilities.wyzszeMorale))
            {
                foreach (var card in row)
                {
                    if (card != moraleCard && !card.isChampion)
                        card.finalStrength += 1;
                }
            }

            // 4. Więź
            var bondGroups = row
                .Where(c => c.ability == Abilities.wiez)
                .GroupBy(c => c.Name);

            foreach (var group in bondGroups)
            {
                int count = group.Count();

                if (count > 1)
                {
                    foreach (var card in group)
                    {
                        if (!card.isChampion)
                            card.finalStrength = card.finalStrength * count;
                    }
                }
            }

            // 5. Sprawdź horn jednostki
            bool unitHornExists = row.Any(c => c.ability == Abilities.rogDowodcyJednostki);
            int hornCount = row.Count(c => c.ability == Abilities.rogDowodcyJednostki);

            if (unitHornExists)
            {
                if (hornCount == 1)
                {
                    foreach (var card in row)
                    {
                        if (card.ability != Abilities.rogDowodcyJednostki)
                            if (!card.isChampion)
                                card.finalStrength *= 2;
                    }
                }
                else
                {
                    foreach (var card in row)
                    {
                        if (!card.isChampion)
                            card.finalStrength *= 2;
                    }
                }
            }
            else if (horn)
            {
                // horn z planszy
                foreach (var card in row)
                {
                    if (!card.isChampion)
                        card.finalStrength *= 2;
                }
            }
        }
    }
}
