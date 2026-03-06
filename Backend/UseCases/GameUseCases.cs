using Backend.Models;
using Backend.Models.Enums;
using System.Collections.Concurrent;

namespace Backend.UseCases
{
    public class GameUseCases
    {
        public ConcurrentDictionary<string, Game> games = new();
        private static readonly Random random = new();

        public static List<T> Shuffle<T>(List<T> list)
        {
            return list.OrderBy(x => random.Next()).ToList();
        }

        public Player ChooseFirstPlayer(string roomId)
        {
            if (!games.TryGetValue(roomId, out var game))
                return null;

            var first = random.Next(2);

            game.CurrentPlayer = first == 0
                ? game.Player1
                : game.Player2;

            return game.CurrentPlayer;
        }

        public Player SetFirstPlayer(string roomId, string chosenPlayerConnectionId)
        {
            var game = games[roomId];

            if (game.Player1.ConnectionId == chosenPlayerConnectionId)
                game.CurrentPlayer = game.Player1;
            else if (game.Player2.ConnectionId == chosenPlayerConnectionId)
                game.CurrentPlayer = game.Player2;
            else
                throw new Exception("Invalid player");

            return game.CurrentPlayer;
        }
    }
}
