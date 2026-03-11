using Backend.Models;
using Backend.Models.Enums;
using Backend.UseCases;
using Xunit;

namespace BackendTests.Tests.UseCasesTests
{
    public class GameUseCasesTests
    {
        private GameUseCases GetGameUseCases() => new GameUseCases();

        private Game CreateGame(string roomId, GameUseCases gameUseCases)
        {
            var game = new Game(
                new List<Card>(),
                new List<Card>(),
                Fractions.Nilfgaard,
                Fractions.NorthernRealms
            )
            {
                Player1 = new Player { Id = 1, ConnectionId = "conn1", Login = "Player1" },
                Player2 = new Player { Id = 2, ConnectionId = "conn2", Login = "Player2" },
                RoomId = roomId
            };

            gameUseCases.games.TryAdd(roomId, game);
            return game;
        }

        [Fact]
        public void Shuffle_ReturnsSameElements()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };

            var result = GameUseCases.Shuffle(list);

            Assert.Equal(list.Count, result.Count);
            Assert.All(list, item => Assert.Contains(item, result));
        }

        [Fact]
        public void Shuffle_DoesNotModifyOriginalList()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var copy = new List<int>(list);

            GameUseCases.Shuffle(list);

            Assert.Equal(copy, list);
        }

        [Fact]
        public void Shuffle_EmptyList_ReturnsEmptyList()
        {
            var list = new List<int>();

            var result = GameUseCases.Shuffle(list);

            Assert.Empty(result);
        }

        [Fact]
        public void Shuffle_SingleElement_ReturnsSingleElement()
        {
            var list = new List<int> { 42 };

            var result = GameUseCases.Shuffle(list);

            Assert.Single(result);
            Assert.Equal(42, result[0]);
        }

        [Fact]
        public void Shuffle_LargeList_ProducesVariedOrder()
        {
            var list = Enumerable.Range(1, 100).ToList();

            var result1 = GameUseCases.Shuffle(list);
            var result2 = GameUseCases.Shuffle(list);

            Assert.False(result1.SequenceEqual(result2));
        }

        [Fact]
        public void ChooseFirstPlayer_ReturnsPlayer1OrPlayer2()
        {
            var uc = GetGameUseCases();
            var game = CreateGame("room1", uc);

            var result = uc.ChooseFirstPlayer("room1");

            Assert.True(
                result.ConnectionId == "conn1" || result.ConnectionId == "conn2"
            );
        }

        [Fact]
        public void ChooseFirstPlayer_SetsCurrentPlayerOnGame()
        {
            var uc = GetGameUseCases();
            var game = CreateGame("room1", uc);

            var result = uc.ChooseFirstPlayer("room1");

            Assert.Equal(result.ConnectionId, game.CurrentPlayer.ConnectionId);
        }

        [Fact]
        public void ChooseFirstPlayer_RoomNotFound_ReturnsNull()
        {
            var uc = GetGameUseCases();

            var result = uc.ChooseFirstPlayer("nonexistent");

            Assert.Null(result);
        }

        [Fact]
        public void ChooseFirstPlayer_OverManyRuns_ReturnsBothPlayers()
        {
            var results = new HashSet<string>();

            for (int i = 0; i < 50; i++)
            {
                var uc = GetGameUseCases();
                CreateGame("room1", uc);
                var player = uc.ChooseFirstPlayer("room1");
                results.Add(player.ConnectionId);
            }
            Assert.Contains("conn1", results);
            Assert.Contains("conn2", results);
        }

        [Fact]
        public void SetFirstPlayer_WithPlayer1ConnectionId_SetsPlayer1()
        {
            var uc = GetGameUseCases();
            var game = CreateGame("room1", uc);

            var result = uc.SetFirstPlayer("room1", "conn1");

            Assert.Equal("conn1", result.ConnectionId);
            Assert.Equal(game.Player1.ConnectionId, game.CurrentPlayer.ConnectionId);
        }

        [Fact]
        public void SetFirstPlayer_WithPlayer2ConnectionId_SetsPlayer2()
        {
            var uc = GetGameUseCases();
            var game = CreateGame("room1", uc);

            var result = uc.SetFirstPlayer("room1", "conn2");

            Assert.Equal("conn2", result.ConnectionId);
            Assert.Equal(game.Player2.ConnectionId, game.CurrentPlayer.ConnectionId);
        }

        [Fact]
        public void SetFirstPlayer_InvalidConnectionId_ThrowsException()
        {
            var uc = GetGameUseCases();
            CreateGame("room1", uc);

            Assert.Throws<Exception>(() => uc.SetFirstPlayer("room1", "invalid_conn"));
        }

        [Fact]
        public void SetFirstPlayer_ReturnsCorrectPlayerObject()
        {
            var uc = GetGameUseCases();
            var game = CreateGame("room1", uc);

            var result = uc.SetFirstPlayer("room1", "conn2");

            Assert.Equal(game.Player2.Id, result.Id);
            Assert.Equal(game.Player2.Login, result.Login);
        }

        [Fact]
        public void SetFirstPlayer_OverwritesPreviousCurrentPlayer()
        {
            var uc = GetGameUseCases();
            var game = CreateGame("room1", uc);

            uc.SetFirstPlayer("room1", "conn1");
            Assert.Equal("conn1", game.CurrentPlayer.ConnectionId);

            uc.SetFirstPlayer("room1", "conn2");
            Assert.Equal("conn2", game.CurrentPlayer.ConnectionId);
        }
    }
}