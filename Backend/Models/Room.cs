namespace Backend.Models
{
    public class Room
    {
        public string Id { get; set; }
        public List<Player> Players { get; set; } = new();
        public int MaxPlayers { get; set; } = 2;
    }
}