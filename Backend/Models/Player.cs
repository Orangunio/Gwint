using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string HashPassword { get; set; }

        [NotMapped]
        public string? ConnectionId { get; set; }
        public ICollection<PlayerDeck> PlayerDecks { get; set; }

        public Player() { }

        public Player(string login, string hashPassword)
        {
            this.Login = login;
            this.HashPassword = hashPassword;
        }
    }
}
