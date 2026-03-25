using Backend.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace Backend.Hubs
{
    public class RoomHub: Hub
    {
        public static ConcurrentDictionary<string, Room> rooms = new();

        public async Task<string> CreateRoom(string name) 
        {
            var roomId = Guid.NewGuid().ToString()[..6].ToUpper(); 
            var room = new Room { Id = roomId };

            room.Players.Add(new Player
            {
                ConnectionId = Context.ConnectionId, 
                Login = name
            });

            rooms.TryAdd(roomId, room);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            return roomId;
        }

        public async Task JoinRoom(string roomId, string name) 
        {
            if (rooms.TryGetValue(roomId, out var room))
            {
                if (!room.Players.Any(p => p.ConnectionId == Context.ConnectionId))
                {
                    room.Players.Add(new Player
                    {
                        ConnectionId = Context.ConnectionId,
                        Login = name
                    });
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                
                await Clients.Group(roomId).SendAsync("PlayerJoined", name);
                
                var playerLogins = room.Players.Select(p => p.Login).ToList();
                await Clients.Caller.SendAsync("RoomUpdate", playerLogins);
            }
            else 
            {
                await Clients.Caller.SendAsync("Error", "Nie znaleziono pokoju o podanym kodzie.");
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
        public async Task BroadcastFraction(string roomId, int fraction)
        {
            if (rooms.TryGetValue(roomId, out var room))
            {
                await Clients.OthersInGroup(roomId).SendAsync("FractionSelected", fraction);
            }
        }
    }
}