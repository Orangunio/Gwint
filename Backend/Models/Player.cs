namespace Backend.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string HashPassword { get; set; }

        private Player() { }

        public Player(string login, string hashPassword)
        {
            this.Login = login;
            this.HashPassword = hashPassword;
        }
    }
}
