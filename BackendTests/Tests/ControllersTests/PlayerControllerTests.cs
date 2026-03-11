using Backend.Controllers;
using Backend.Database;
using Backend.Models;
using Backend.Models.Body;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Backend.Tests.ControllersTests
{
    public class PlayerControllerTests
    {
        private GwintDBContext GetDbContext(string dbName = "PlayerTestDB")
        {
            var options = new DbContextOptionsBuilder<GwintDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new GwintDBContext(options);
        }

        private IConfiguration GetConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                { "Jwt:SecretKey", "super-secret-key-that-is-long-enough-32b" },
                { "Jwt:Issuer", "gwint-api" },
                { "Jwt:Audience", "gwint-client" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task Register_NewUser_ReturnsOkWithPlayer()
        {
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PlayerController(db);

            var body = new AuthBody { Login = "testuser", Password = "Password123!" };

            var result = await controller.CreatePlayer(body) as OkObjectResult;

            Assert.NotNull(result);
            var player = result.Value as Player;
            Assert.NotNull(player);
            Assert.Equal("testuser", player.Login);
        }

        [Fact]
        public async Task Register_DuplicateLogin_ReturnsBadRequest()
        {
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PlayerController(db);

            var body = new AuthBody { Login = "duplicateuser", Password = "Password123!" };

            await controller.CreatePlayer(body);
            var result = await controller.CreatePlayer(body);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_SavesToDatabase()
        {
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PlayerController(db);

            var body = new AuthBody { Login = "saveduser", Password = "Password123!" };

            await controller.CreatePlayer(body);

            Assert.Single(db.Players.Where(p => p.Login == "saveduser"));
        }

        [Fact]
        public async Task Register_PasswordIsHashed()
        {
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PlayerController(db);

            var body = new AuthBody { Login = "hashuser", Password = "PlainPassword" };

            await controller.CreatePlayer(body);

            var player = db.Players.First(p => p.Login == "hashuser");
            Assert.NotEqual("PlainPassword", player.HashPassword);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PlayerController(db);
            var config = GetConfiguration();

            var registerBody = new AuthBody { Login = "loginuser", Password = "Password123!" };
            await controller.CreatePlayer(registerBody);

            var loginBody = new AuthBody { Login = "loginuser", Password = "Password123!" };
            var result = await controller.Login(loginBody, config) as OkObjectResult;

            Assert.NotNull(result);
            var tokenObj = result.Value;
            Assert.NotNull(tokenObj);

            var tokenProp = tokenObj.GetType().GetProperty("token");
            Assert.NotNull(tokenProp);

            var tokenValue = tokenProp.GetValue(tokenObj) as string;
            Assert.False(string.IsNullOrWhiteSpace(tokenValue));
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsBadRequest()
        {
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PlayerController(db);
            var config = GetConfiguration();

            var registerBody = new AuthBody { Login = "wrongpassuser", Password = "CorrectPass123!" };
            await controller.CreatePlayer(registerBody);

            var loginBody = new AuthBody { Login = "wrongpassuser", Password = "WrongPass!" };
            var result = await controller.Login(loginBody, config);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_NonExistentUser_ReturnsBadRequest()
        {
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PlayerController(db);
            var config = GetConfiguration();

            var body = new AuthBody { Login = "ghost", Password = "whatever" };
            var result = await controller.Login(body, config);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_NullBody_ReturnsBadRequest()
        {
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PlayerController(db);
            var config = GetConfiguration();

            var result = await controller.Login(new AuthBody { Login = "", Password = "" }, config);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_EmptyLogin_ReturnsBadRequest()
        {
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PlayerController(db);
            var config = GetConfiguration();

            var body = new AuthBody { Login = "", Password = "somepassword" };
            var result = await controller.Login(body, config);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_EmptyPassword_ReturnsBadRequest()
        {
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PlayerController(db);
            var config = GetConfiguration();

            var body = new AuthBody { Login = "someuser", Password = "" };
            var result = await controller.Login(body, config);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}