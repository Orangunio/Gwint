using Backend.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace Backend.Hubs
{
    public class RoomHub: Hub
    {
        public static ConcurrentDictionary<string, Room> rooms = new();

        public string CreateRoom(string connectionId, string name)
        {
            var roomId = Guid.NewGuid().ToString()[..6];

            var room = new Room { Id = roomId };

            room.Players.Add(new Player
            {
                ConnectionId = connectionId,
                Login = name
            });

            rooms.TryAdd(roomId, room);

            return roomId;
        }

        public async Task JoinRoom(string roomId, string connectionId, string name)
        {
            if (rooms.TryGetValue(roomId, out var room))
            {
                room.Players.Add(new Player
                {
                    ConnectionId = connectionId,
                    Login = name
                });

                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

                await Clients.Group(roomId).SendAsync("PlayerJoined");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var room in rooms.Values)
            {
                var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player != null)
                {
                    room.Players.Remove(player);
                    await Clients.Group(room.Id).SendAsync("PlayerLeft", player.Login);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task StartGame(string roomId)
        {
            if (rooms.TryGetValue(roomId, out var room))
            {
                await Clients.Group(roomId).SendAsync("GameStarted", roomId);

                var playerLogins = room.Players.Select(p => p.Login).ToList();
                await Clients.Group(roomId).SendAsync("GameSetup", playerLogins);
            }
        }
    }
}