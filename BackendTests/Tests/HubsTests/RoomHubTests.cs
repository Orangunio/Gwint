using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System.Threading.Tasks;

namespace BackendTests.Tests.HubsTests
{
    public class RoomHubTests
    {
        // Aktualne API hubów: CreateRoom(name) i JoinRoom(roomId, name) korzystają z Context.ConnectionId,
        // a nie z parametru. Każdy test musi zatem zasymulować HubCallerContext.

        private RoomHub GetHub(string connectionId, out Mock<IHubCallerClients> clientsMock,
            out Mock<IGroupManager> groupsMock, out Mock<IClientProxy> groupProxyMock)
        {
            clientsMock = new Mock<IHubCallerClients>();
            groupsMock = new Mock<IGroupManager>();
            groupProxyMock = new Mock<IClientProxy>();

            // Każde wywołanie Group(...) zwraca ten sam mock proxy – wystarczy do weryfikacji SendCoreAsync.
            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(groupProxyMock.Object);
            groupProxyMock
                .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);

            // Caller też potrzebny dla tras błędów (RoomNotFound itp.)
            var callerProxyMock = new Mock<ISingleClientProxy>();
            callerProxyMock
                .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Caller).Returns(callerProxyMock.Object);

            groupsMock.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            groupsMock.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);

            var contextMock = new Mock<HubCallerContext>();
            contextMock.Setup(c => c.ConnectionId).Returns(connectionId);

            var hub = new RoomHub
            {
                Clients = clientsMock.Object,
                Groups = groupsMock.Object,
                Context = contextMock.Object
            };

            RoomHub.rooms.Clear();
            return hub;
        }

        [Fact]
        public async Task CreateRoom_ShouldAddRoomWithPlayer()
        {
            var hub = GetHub("conn1", out _, out _, out _);

            var roomId = await hub.CreateRoom("Alice");

            Assert.True(RoomHub.rooms.ContainsKey(roomId));
            var room = RoomHub.rooms[roomId];
            Assert.Single(room.Players);
            Assert.Equal("Alice", room.Players[0].Login);
            Assert.Equal("conn1", room.Players[0].ConnectionId);
        }

        [Fact]
        public async Task JoinRoom_ShouldAddPlayerAndNotifyGroup()
        {
            // Host tworzy pokój
            var hostHub = GetHub("conn1", out _, out _, out _);
            var roomId = await hostHub.CreateRoom("Alice");

            // Drugi hub – nowy ConnectionId – ten sam ConcurrentDictionary statyczny w RoomHub
            var clientsMock = new Mock<IHubCallerClients>();
            var groupsMock = new Mock<IGroupManager>();
            var groupProxyMock = new Mock<IClientProxy>();

            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(groupProxyMock.Object);
            groupProxyMock
                .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);

            var callerMock = new Mock<ISingleClientProxy>();
            callerMock.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);
            clientsMock.Setup(c => c.Caller).Returns(callerMock.Object);

            groupsMock.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);

            var contextMock = new Mock<HubCallerContext>();
            contextMock.Setup(c => c.ConnectionId).Returns("conn2");

            var guestHub = new RoomHub
            {
                Clients = clientsMock.Object,
                Groups = groupsMock.Object,
                Context = contextMock.Object
            };

            await guestHub.JoinRoom(roomId, "Bob");

            var room = RoomHub.rooms[roomId];
            Assert.Equal(2, room.Players.Count);
            Assert.Contains(room.Players, p => p.Login == "Bob" && p.ConnectionId == "conn2");

            groupsMock.Verify(g => g.AddToGroupAsync("conn2", roomId, default), Times.Once);
            groupProxyMock.Verify(
                p => p.SendCoreAsync("PlayersUpdated", It.IsAny<object[]>(), default),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task OnDisconnectedAsync_ShouldRemovePlayerAndNotifyGroup()
        {
            var hub = GetHub("conn1", out _, out _, out var groupProxyMock);
            var roomId = await hub.CreateRoom("Alice");

            await hub.OnDisconnectedAsync(null);

            var room = RoomHub.rooms[roomId];
            Assert.Empty(room.Players);

            groupProxyMock.Verify(
                p => p.SendCoreAsync("PlayersUpdated", It.IsAny<object[]>(), default),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task StartGame_ShouldSendGameStartedAndSetup()
        {
            var hub = GetHub("conn1", out _, out _, out var groupProxyMock);
            var roomId = await hub.CreateRoom("Alice");

            await hub.StartGame(roomId);

            groupProxyMock.Verify(
                p => p.SendCoreAsync("GameStarted", It.IsAny<object[]>(), default),
                Times.AtLeastOnce);
            groupProxyMock.Verify(
                p => p.SendCoreAsync("GameSetup", It.IsAny<object[]>(), default),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task CreateRoom_ShouldGenerateUniqueRoomIds()
        {
            var hub1 = GetHub("conn1", out _, out _, out _);
            var roomId1 = await hub1.CreateRoom("Alice");

            var hub2 = GetHub("conn2", out _, out _, out _);
            // GetHub czyści RoomHub.rooms; pierwszy roomId zachowamy w zmiennej, więc i tak różne.
            var roomId2 = await hub2.CreateRoom("Bob");

            Assert.NotEqual(roomId1, roomId2);
        }
    }
}
