using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendTests.Tests.HubsTests
{
    public class RoomHubTests
    {
        private RoomHub GetHub(out Mock<IHubCallerClients> clientsMock, out Mock<IGroupManager> groupsMock)
        {
            clientsMock = new Mock<IHubCallerClients>();
            groupsMock = new Mock<IGroupManager>();

            var hub = new RoomHub
            {
                Clients = clientsMock.Object,
                Groups = groupsMock.Object
            };

            RoomHub.rooms.Clear();

            return hub;
        }

        [Fact]
        public void CreateRoom_ShouldAddRoomWithPlayer()
        {
            var hub = GetHub(out _, out _);
            string connectionId = "conn1";
            string playerName = "Alice";

            var roomId = hub.CreateRoom(connectionId, playerName);

            Assert.True(RoomHub.rooms.ContainsKey(roomId));
            var room = RoomHub.rooms[roomId];
            Assert.Single(room.Players);
            Assert.Equal(playerName, room.Players[0].Login);
            Assert.Equal(connectionId, room.Players[0].ConnectionId);
        }

        [Fact]
        public async Task JoinRoom_ShouldAddPlayerAndNotifyGroup()
        {
            var hub = GetHub(out var clientsMock, out var groupsMock);

            string roomId = hub.CreateRoom("conn1", "Alice");

            var contextMock = new Mock<HubCallerContext>();
            contextMock.Setup(c => c.ConnectionId).Returns("conn2");
            hub.Context = contextMock.Object;

            bool playerJoinedCalled = false;
            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync("PlayerJoined", It.IsAny<object[]>(), default))
                .Callback(() => playerJoinedCalled = true)
                .Returns(Task.CompletedTask);

            clientsMock.Setup(c => c.Group(roomId)).Returns(clientProxyMock.Object);

            groupsMock.Setup(g => g.AddToGroupAsync("conn2", roomId, default))
                      .Returns(Task.CompletedTask);

            await hub.JoinRoom(roomId, "conn2", "Bob");

            var room = RoomHub.rooms[roomId];
            Assert.Equal(2, room.Players.Count);
            Assert.Contains(room.Players, p => p.Login == "Bob");

            Assert.True(playerJoinedCalled);

            groupsMock.Verify(g => g.AddToGroupAsync("conn2", roomId, default), Times.Once);
        }

        [Fact]
        public async Task OnDisconnectedAsync_ShouldRemovePlayerAndNotifyGroup()
        {
            var hub = GetHub(out var clientsMock, out _);

            string roomId = hub.CreateRoom("conn1", "Alice");

            bool playerLeftCalled = false;
            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync("PlayerLeft", It.IsAny<object[]>(), default))
                .Callback(() => playerLeftCalled = true)
                .Returns(Task.CompletedTask);

            clientsMock.Setup(c => c.Group(roomId)).Returns(clientProxyMock.Object);

            var contextMock = new Mock<HubCallerContext>();
            contextMock.Setup(c => c.ConnectionId).Returns("conn1");
            hub.Context = contextMock.Object;

            await hub.OnDisconnectedAsync(null);

            var room = RoomHub.rooms[roomId];
            Assert.Empty(room.Players);

            Assert.True(playerLeftCalled);
        }

        [Fact]
        public async Task StartGame_ShouldSendGameStartedAndSetup()
        {
            var hub = GetHub(out var clientsMock, out _);

            string roomId = hub.CreateRoom("conn1", "Alice");

            hub.CreateRoom("conn2", "Bob");

            bool gameStartedCalled = false;
            bool gameSetupCalled = false;
            var clientProxyMock = new Mock<IClientProxy>();
            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync("GameStarted", It.IsAny<object[]>(), default))
                .Callback(() => gameStartedCalled = true)
                .Returns(Task.CompletedTask);

            clientProxyMock
                .Setup(proxy => proxy.SendCoreAsync("GameSetup", It.IsAny<object[]>(), default))
                .Callback(() => gameSetupCalled = true)
                .Returns(Task.CompletedTask);

            clientsMock.Setup(c => c.Group(roomId)).Returns(clientProxyMock.Object);

            await hub.StartGame(roomId);

            Assert.True(gameStartedCalled, "GameStarted event was not sent");
            Assert.True(gameSetupCalled, "GameSetup event was not sent");
        }

        [Fact]
        public void CreateRoom_ShouldGenerateUniqueRoomIds()
        {
            var hub = GetHub(out _, out _);

            string roomId1 = hub.CreateRoom("conn1", "Alice");
            string roomId2 = hub.CreateRoom("conn2", "Bob");

            Assert.NotEqual(roomId1, roomId2);
        }
    }
}
