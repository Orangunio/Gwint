using Backend.Models;

namespace Backend.UseCases
{
    public class BoardUseCases
    {
        public void AddPoints(Board board, int playerIdx, int rowIdx, int points)
        {
            // playerIdx: 0 (P1) lub 1 (P2)
            // rowIdx: 0 (R1), 1 (R2) lub 2 (R3)
            board.RowScores[playerIdx, rowIdx] += points;
        }
        public int GetPlayerTotal(Board board, int playerIdx)
        {
            // Sumujemy indeksy 0, 1 i 2 dla wybranego gracza
            return board.RowScores[playerIdx, 0] +
                   board.RowScores[playerIdx, 1] +
                   board.RowScores[playerIdx, 2];
        }
        public void Reset(Board board)
        {
            for (int p = 0; p < 2; p++)
            {
                for (int r = 0; r < 3; r++)
                {
                    board.RowScores[p, r] = 0;
                }
            }
        }
    }
}
