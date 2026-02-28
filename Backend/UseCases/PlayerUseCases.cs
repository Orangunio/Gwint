using Backend.Models;
using BCrypt.Net;

namespace Backend.UseCases
{
    public class PlayerUseCases
    {
        public PlayerUseCases()
        {
        }
        public Player CreatePlayer(string login, string password)
        {
            var hashPassword = HashPassword(password);

            Player player = new Player(login, hashPassword);
            return player;
        }

        public bool VerifyPassword(string password, string passwordInDb)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordInDb);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}