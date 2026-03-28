using Backend.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace Backend.Hubs
{
    public class RoomHub: Hub
    {
        public static ConcurrentDictionary<string, Room> rooms = new();

        public async Task GetPlayers(string roomId)
        {
            if (!rooms.TryGetValue(roomId, out var room))
            {
                await Clients.Caller.SendAsync("RoomNotFound", roomId);
                return;
            }
            var logins = room.Players.Select(p => p.Login).ToList();
            await Clients.Caller.SendAsync("PlayersUpdated", logins);
        }

        public async Task<string> CreateRoom(string name)
        {
            var roomId = Guid.NewGuid().ToString()[..6];
            var room = new Room { Id = roomId };
            room.Players.Add(new Player
            {
                ConnectionId = Context.ConnectionId,
                Login = name
            });
            rooms.TryAdd(roomId, room);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            var logins = room.Players.Select(p => p.Login).ToList();
            await Clients.Group(roomId).SendAsync("PlayersUpdated", logins);
            return roomId;
        }

        public async Task JoinRoom(string roomId, string name)
        {
            if (!rooms.TryGetValue(roomId, out var room))
            {
                await Clients.Caller.SendAsync("RoomNotFound", roomId);
                return;
            }

            if (room.Players.Count >= 2)
            {
                await Clients.Caller.SendAsync("RoomFull", roomId);
                return;
            }

            if (!room.Players.Any(p => p.ConnectionId == Context.ConnectionId))
            {
                room.Players.Add(new Player
                {
                    ConnectionId = Context.ConnectionId,
                    Login = name
                });
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            }

            var logins = room.Players.Select(p => p.Login).ToList();
            await Clients.Group(roomId).SendAsync("PlayersUpdated", logins);
        }


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var room in rooms.Values)
            {
                var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player != null)
                {
                    room.Players.Remove(player);
                    var logins = room.Players.Select(p => p.Login).ToList();
                    await Clients.Group(room.Id).SendAsync("PlayersUpdated", logins);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task LeaveRoom(string roomId)
        {
            if (!rooms.TryGetValue(roomId, out var room))
                return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player != null)
            {
                room.Players.Remove(player);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

                if (room.Players.Count == 0)
                {
                    rooms.TryRemove(roomId, out _);
                }
                else
                {
                    var logins = room.Players.Select(p => p.Login).ToList();
                    await Clients.Group(roomId).SendAsync("PlayersUpdated", logins);
                }
            }
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
        public async Task BroadcastFraction(string roomId, int fraction)
        {
            if (rooms.TryGetValue(roomId, out var room))
            {
                await Clients.OthersInGroup(roomId).SendAsync("FractionSelected", fraction);
            }
        }
    }
}