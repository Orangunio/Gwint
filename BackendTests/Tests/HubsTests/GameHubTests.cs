using Backend.Database;
using Backend.Hubs;
using Backend.Models;
using Backend.Models.Enums;
using Backend.UseCases;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BackendTests.Tests.HubsTests
{
    public class GameHubTests
    {
        private GwintDBContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<GwintDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new GwintDBContext(options);
        }

        private GameHub GetHub(GwintDBContext db, GameUseCases gameUseCases,
            out Mock<IHubCallerClients> clientsMock)
        {
            clientsMock = new Mock<IHubCallerClients>();

            var hub = new GameHub(gameUseCases, db)
            {
                Clients = clientsMock.Object
            };

            RoomHub.rooms.Clear();
            gameUseCases.games.Clear();

            return hub;
        }

        private GwintDBContext SeedDatabase(string dbName, int player1Id, int player2Id,
            Fractions p1Fraction, Fractions p2Fraction)
        {
            var db = GetDbContext(dbName);

            var commander1 = new Card("Commander1", p1Fraction, Abilities.brak, 10, Place.FirstRow, true, true, false);
            db.Cards.Add(commander1);
            db.SaveChanges();
            db.PlayerDecks.Add(new PlayerDeck { PlayerId = player1Id, CardId = commander1.Id });

            for (int i = 0; i < 19; i++)
            {
                var card = new Card($"P1Card{i}", p1Fraction, Abilities.brak, 5, Place.FirstRow, false, false, false);
                db.Cards.Add(card);
                db.SaveChanges();
                db.PlayerDecks.Add(new PlayerDeck { PlayerId = player1Id, CardId = card.Id });
            }

            var commander2 = new Card("Commander2", p2Fraction, Abilities.brak, 10, Place.FirstRow, true, true, false);
            db.Cards.Add(commander2);
            db.SaveChanges();
            db.PlayerDecks.Add(new PlayerDeck { PlayerId = player2Id, CardId = commander2.Id });

            for (int i = 0; i < 19; i++)
            {
                var card = new Card($"P2Card{i}", p2Fraction, Abilities.brak, 5, Place.FirstRow, false, false, false);
                db.Cards.Add(card);
                db.SaveChanges();
                db.PlayerDecks.Add(new PlayerDeck { PlayerId = player2Id, CardId = card.Id });
            }

            db.SaveChanges();
            return db;
        }
        [Fact]
        public async Task StartGame_RoomNotFound_DoesNotSendGameStarted()
        {
            var gameUseCases = new GameUseCases();
            var db = GetDbContext(Guid.NewGuid().ToString());
            var hub = GetHub(db, gameUseCases, out var clientsMock);

            bool gameStartedCalled = false;
            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync("GameStarted", It.IsAny<object[]>(), default))
                .Callback(() => gameStartedCalled = true)
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);

            await hub.StartGame("nonexistent", Fractions.Nilfgaard, Fractions.NorthernRealms);

            Assert.False(gameStartedCalled);
        }

        [Fact]
        public async Task StartGame_RoomWithOnePlayer_DoesNotSendGameStarted()
        {
            var gameUseCases = new GameUseCases();
            var db = GetDbContext(Guid.NewGuid().ToString());
            var hub = GetHub(db, gameUseCases, out var clientsMock);

            bool gameStartedCalled = false;
            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync("GameStarted", It.IsAny<object[]>(), default))
                .Callback(() => gameStartedCalled = true)
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(clientProxyMock.Object);

            var room = new Room { Id = "room1" };
            room.Players.Add(new Player { Id = 1, ConnectionId = "conn1", Login = "Player1" });
            RoomHub.rooms.TryAdd("room1", room);

            await hub.StartGame("room1", Fractions.Nilfgaard, Fractions.NorthernRealms);

            Assert.False(gameStartedCalled);
        }

        [Fact]
        public async Task StartGame_AssignsCommanderCards()
        {
            var gameUseCases = new GameUseCases();
            var db = SeedDatabase(Guid.NewGuid().ToString(), 1, 2, Fractions.Nilfgaard, Fractions.NorthernRealms);
            var hub = GetHub(db, gameUseCases, out var clientsMock);

            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(clientProxyMock.Object);

            var room = new Room { Id = "room1" };
            room.Players.Add(new Player { Id = 1, ConnectionId = "conn1", Login = "Player1" });
            room.Players.Add(new Player { Id = 2, ConnectionId = "conn2", Login = "Player2" });
            RoomHub.rooms.TryAdd("room1", room);

            await hub.StartGame("room1", Fractions.Nilfgaard, Fractions.NorthernRealms);

            gameUseCases.games.TryGetValue("room1", out var game);
            Assert.NotNull(game.Player1CommanderCard);
            Assert.NotNull(game.Player2CommanderCard);
            Assert.True(game.Player1CommanderCard.isCommander);
            Assert.True(game.Player2CommanderCard.isCommander);
        }

        [Fact]
        public async Task StartGame_DealsTenCardsToEachPlayer()
        {
            var gameUseCases = new GameUseCases();
            var db = SeedDatabase(Guid.NewGuid().ToString(), 1, 2, Fractions.Nilfgaard, Fractions.NorthernRealms);
            var hub = GetHub(db, gameUseCases, out var clientsMock);

            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(clientProxyMock.Object);

            var room = new Room { Id = "room1" };
            room.Players.Add(new Player { Id = 1, ConnectionId = "conn1", Login = "Player1" });
            room.Players.Add(new Player { Id = 2, ConnectionId = "conn2", Login = "Player2" });
            RoomHub.rooms.TryAdd("room1", room);

            await hub.StartGame("room1", Fractions.Nilfgaard, Fractions.NorthernRealms);

            gameUseCases.games.TryGetValue("room1", out var game);
            Assert.Equal(10, game.Player1CardsOnHand.Count);
            Assert.Equal(10, game.Player2CardsOnHand.Count);
        }

        [Fact]
        public async Task StartGame_CommanderNotInDeck()
        {
            var gameUseCases = new GameUseCases();
            var db = SeedDatabase(Guid.NewGuid().ToString(), 1, 2, Fractions.Nilfgaard, Fractions.NorthernRealms);
            var hub = GetHub(db, gameUseCases, out var clientsMock);

            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(clientProxyMock.Object);

            var room = new Room { Id = "room1" };
            room.Players.Add(new Player { Id = 1, ConnectionId = "conn1", Login = "Player1" });
            room.Players.Add(new Player { Id = 2, ConnectionId = "conn2", Login = "Player2" });
            RoomHub.rooms.TryAdd("room1", room);

            await hub.StartGame("room1", Fractions.Nilfgaard, Fractions.NorthernRealms);

            gameUseCases.games.TryGetValue("room1", out var game);
            Assert.DoesNotContain(game.Player1CardInDeck, c => c.isCommander);
            Assert.DoesNotContain(game.Player2CardInDeck, c => c.isCommander);
        }

        // ─── ChooseFirstPlayer ────────────────────────────────────────────────────

        [Fact]
        public async Task ChooseFirstPlayer_BothNonScoia_SendsTurnStarted()
        {
            var gameUseCases = new GameUseCases();
            var db = SeedDatabase(Guid.NewGuid().ToString(), 1, 2, Fractions.Nilfgaard, Fractions.NorthernRealms);
            var hub = GetHub(db, gameUseCases, out var clientsMock);

            var setupProxy = new Mock<IClientProxy>();
            setupProxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(setupProxy.Object);

            var room = new Room { Id = "room1" };
            room.Players.Add(new Player { Id = 1, ConnectionId = "conn1", Login = "Player1" });
            room.Players.Add(new Player { Id = 2, ConnectionId = "conn2", Login = "Player2" });
            RoomHub.rooms.TryAdd("room1", room);
            await hub.StartGame("room1", Fractions.Nilfgaard, Fractions.NorthernRealms);

            bool turnStartedCalled = false;
            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync("TurnStarted", It.IsAny<object[]>(), default))
                .Callback(() => turnStartedCalled = true)
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(clientProxyMock.Object);

            await hub.ChooseFirstPlayer("room1");

            Assert.True(turnStartedCalled);
        }

        [Fact]
        public async Task ChooseFirstPlayer_OnlyPlayer1IsScoia_SendsScoiataelChooseFirstPlayerToPlayer1()
        {
            var gameUseCases = new GameUseCases();
            var db = SeedDatabase(Guid.NewGuid().ToString(), 1, 2, Fractions.ScoiaTael, Fractions.NorthernRealms);
            var hub = GetHub(db, gameUseCases, out var clientsMock);

            var setupProxy = new Mock<IClientProxy>();
            setupProxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(setupProxy.Object);

            var room = new Room { Id = "room1" };
            room.Players.Add(new Player { Id = 1, ConnectionId = "conn1", Login = "Player1" });
            room.Players.Add(new Player { Id = 2, ConnectionId = "conn2", Login = "Player2" });
            RoomHub.rooms.TryAdd("room1", room);
            await hub.StartGame("room1", Fractions.ScoiaTael, Fractions.NorthernRealms);

            bool scoiaCalled = false;
            var clientProxyMock = new Mock<ISingleClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync("ScoiataelChooseFirstPlayer", It.IsAny<object[]>(), default))
                .Callback(() => scoiaCalled = true)
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Client("conn1")).Returns(clientProxyMock.Object);

            await hub.ChooseFirstPlayer("room1");

            Assert.True(scoiaCalled);
        }

        [Fact]
        public async Task ChooseFirstPlayer_BothScoia_SendsTurnStarted()
        {
            var gameUseCases = new GameUseCases();
            var db = SeedDatabase(Guid.NewGuid().ToString(), 1, 2, Fractions.ScoiaTael, Fractions.ScoiaTael);
            var hub = GetHub(db, gameUseCases, out var clientsMock);

            var setupProxy = new Mock<IClientProxy>();
            setupProxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(setupProxy.Object);

            var room = new Room { Id = "room1" };
            room.Players.Add(new Player { Id = 1, ConnectionId = "conn1", Login = "Player1" });
            room.Players.Add(new Player { Id = 2, ConnectionId = "conn2", Login = "Player2" });
            RoomHub.rooms.TryAdd("room1", room);
            await hub.StartGame("room1", Fractions.ScoiaTael, Fractions.ScoiaTael);

            bool turnStartedCalled = false;
            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync("TurnStarted", It.IsAny<object[]>(), default))
                .Callback(() => turnStartedCalled = true)
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(clientProxyMock.Object);

            await hub.ChooseFirstPlayer("room1");

            Assert.True(turnStartedCalled);
        }

        [Fact]
        public async Task SetFirstPlayer_ValidPlayer_SendsTurnStarted()
        {
            var gameUseCases = new GameUseCases();
            var db = SeedDatabase(Guid.NewGuid().ToString(), 1, 2, Fractions.Nilfgaard, Fractions.NorthernRealms);
            var hub = GetHub(db, gameUseCases, out var clientsMock);

            var setupProxy = new Mock<IClientProxy>();
            setupProxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(setupProxy.Object);

            var room = new Room { Id = "room1" };
            room.Players.Add(new Player { Id = 1, ConnectionId = "conn1", Login = "Player1" });
            room.Players.Add(new Player { Id = 2, ConnectionId = "conn2", Login = "Player2" });
            RoomHub.rooms.TryAdd("room1", room);
            await hub.StartGame("room1", Fractions.Nilfgaard, Fractions.NorthernRealms);

            bool turnStartedCalled = false;
            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync("TurnStarted", It.IsAny<object[]>(), default))
                .Callback(() => turnStartedCalled = true)
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(clientProxyMock.Object);

            await hub.SetFirstPlayer("room1", "conn1");

            Assert.True(turnStartedCalled);
        }

        [Fact]
        public async Task SetFirstPlayer_SetsCurrentPlayerCorrectly()
        {
            var gameUseCases = new GameUseCases();
            var db = SeedDatabase(Guid.NewGuid().ToString(), 1, 2, Fractions.Nilfgaard, Fractions.NorthernRealms);
            var hub = GetHub(db, gameUseCases, out var clientsMock);

            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(clientProxyMock.Object);

            var room = new Room { Id = "room1" };
            room.Players.Add(new Player { Id = 1, ConnectionId = "conn1", Login = "Player1" });
            room.Players.Add(new Player { Id = 2, ConnectionId = "conn2", Login = "Player2" });
            RoomHub.rooms.TryAdd("room1", room);
            await hub.StartGame("room1", Fractions.Nilfgaard, Fractions.NorthernRealms);

            await hub.SetFirstPlayer("room1", "conn2");

            gameUseCases.games.TryGetValue("room1", out var game);
            Assert.Equal("conn2", game.CurrentPlayer.ConnectionId);
        }

        [Fact]
        public async Task PlayCard_NormalCard_SwitchesCurrentPlayer()
        {
            var gameUseCases = new GameUseCases();
            var db = SeedDatabase(Guid.NewGuid().ToString(), 1, 2, Fractions.Nilfgaard, Fractions.NorthernRealms);
            var hub = GetHub(db, gameUseCases, out var clientsMock);

            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(clientProxyMock.Object);

            var room = new Room { Id = "room1" };
            room.Players.Add(new Player { Id = 1, ConnectionId = "conn1", Login = "Player1" });
            room.Players.Add(new Player { Id = 2, ConnectionId = "conn2", Login = "Player2" });
            RoomHub.rooms.TryAdd("room1", room);
            await hub.StartGame("room1", Fractions.Nilfgaard, Fractions.NorthernRealms);
            await hub.SetFirstPlayer("room1", "conn1");

            var normalCard = new Card("Unit", Fractions.Nilfgaard, Abilities.brak, 5, Place.FirstRow, false, false, false);
            await hub.PlayCard("room1", normalCard);

            gameUseCases.games.TryGetValue("room1", out var game);
            Assert.Equal("conn2", game.CurrentPlayer.ConnectionId);
        }

        [Fact]
        public async Task PlayCard_SendsNextTurn()
        {
            var gameUseCases = new GameUseCases();
            var db = SeedDatabase(Guid.NewGuid().ToString(), 1, 2, Fractions.Nilfgaard, Fractions.NorthernRealms);
            var hub = GetHub(db, gameUseCases, out var clientsMock);

            var setupProxy = new Mock<IClientProxy>();
            setupProxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(setupProxy.Object);

            var room = new Room { Id = "room1" };
            room.Players.Add(new Player { Id = 1, ConnectionId = "conn1", Login = "Player1" });
            room.Players.Add(new Player { Id = 2, ConnectionId = "conn2", Login = "Player2" });
            RoomHub.rooms.TryAdd("room1", room);
            await hub.StartGame("room1", Fractions.Nilfgaard, Fractions.NorthernRealms);
            await hub.SetFirstPlayer("room1", "conn1");

            bool nextTurnCalled = false;
            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync("NextTurn", It.IsAny<object[]>(), default))
                .Callback(() => nextTurnCalled = true)
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(clientProxyMock.Object);

            var normalCard = new Card("Unit", Fractions.Nilfgaard, Abilities.brak, 5, Place.FirstRow, false, false, false);
            await hub.PlayCard("room1", normalCard);

            Assert.True(nextTurnCalled);
        }

        [Fact]
        public async Task PlayCard_Player2Passed_CurrentPlayerRemainsPlayer1()
        {
            var gameUseCases = new GameUseCases();
            var db = SeedDatabase(Guid.NewGuid().ToString(), 1, 2, Fractions.Nilfgaard, Fractions.NorthernRealms);
            var hub = GetHub(db, gameUseCases, out var clientsMock);

            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(clientProxyMock.Object);

            var room = new Room { Id = "room1" };
            room.Players.Add(new Player { Id = 1, ConnectionId = "conn1", Login = "Player1" });
            room.Players.Add(new Player { Id = 2, ConnectionId = "conn2", Login = "Player2" });
            RoomHub.rooms.TryAdd("room1", room);
            await hub.StartGame("room1", Fractions.Nilfgaard, Fractions.NorthernRealms);
            await hub.SetFirstPlayer("room1", "conn1");

            gameUseCases.games.TryGetValue("room1", out var game);
            game.Player2Passed = true;

            var normalCard = new Card("Unit", Fractions.Nilfgaard, Abilities.brak, 5, Place.FirstRow, false, false, false);
            await hub.PlayCard("room1", normalCard);

            Assert.Equal("conn1", game.CurrentPlayer.ConnectionId);
        }

        [Fact]
        public async Task PlayCard_Player1Passed_CurrentPlayerRemainsPlayer2()
        {
            var gameUseCases = new GameUseCases();
            var db = SeedDatabase(Guid.NewGuid().ToString(), 1, 2, Fractions.Nilfgaard, Fractions.NorthernRealms);
            var hub = GetHub(db, gameUseCases, out var clientsMock);

            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Group("room1")).Returns(clientProxyMock.Object);

            var room = new Room { Id = "room1" };
            room.Players.Add(new Player { Id = 1, ConnectionId = "conn1", Login = "Player1" });
            room.Players.Add(new Player { Id = 2, ConnectionId = "conn2", Login = "Player2" });
            RoomHub.rooms.TryAdd("room1", room);
            await hub.StartGame("room1", Fractions.Nilfgaard, Fractions.NorthernRealms);
            await hub.SetFirstPlayer("room1", "conn2");

            gameUseCases.games.TryGetValue("room1", out var game);
            game.Player1Passed = true;

            var normalCard = new Card("Unit", Fractions.NorthernRealms, Abilities.brak, 5, Place.FirstRow, false, false, false);
            await hub.PlayCard("room1", normalCard);

            Assert.Equal("conn2", game.CurrentPlayer.ConnectionId);
        }
    }
}