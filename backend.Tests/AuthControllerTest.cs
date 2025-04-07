using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Worktest.backend.Controllers;
using Worktest.backend.Models;
using Xunit;

namespace Worktest.Tests
{
    /// <summary>
    /// Contains unit tests for the AuthController class.
    /// Tests various scenarios for user registration and login.
    /// </summary>
    public class AuthControllerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthControllerTests"/> class.
        /// Configures in-memory database options for testing.
        /// </summary>
        public AuthControllerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        }

        /// <summary>
        /// Creates a mock configuration for JWT settings used in the controller.
        /// </summary>
        /// <returns>A fake configuration object.</returns>
        private IConfiguration GetFakeConfig()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"JwtSettings:SecretKey", "ThisIsASecretKeyForJwtTesting123!"},
                {"JwtSettings:Issuer", "TestIssuer"},
                {"JwtSettings:Audience", "TestAudience"},
                {"JwtSettings:ExpirationInMinutes", "30"},
            };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        /// <summary>
        /// Test case to verify the behavior when a new user registers.
        /// The test expects a successful registration (OkObjectResult).
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task Register_ReturnsOk_WhenUserIsNew()
        {
            using var context = new ApplicationDbContext(_dbOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var controller = new AuthController(context, GetFakeConfig());
            var model = new RegisterModel { Username = "newuser", Password = "Test123!" };

            var result = await controller.Register(model);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Registration successful", okResult.Value.ToString());
        }

        /// <summary>
        /// Test case to verify the behavior when a user with the same username already exists.
        /// The test expects a ConflictObjectResult with a "Username already exists" message.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task Register_ReturnsConflict_WhenUserExists()
        {
            using var context = new ApplicationDbContext(_dbOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var hashed = BCrypt.Net.BCrypt.HashPassword("Test123!");
            context.Users.Add(new User { Username = "existinguser", PasswordHash = hashed, Role = "User" });
            await context.SaveChangesAsync();

            var controller = new AuthController(context, GetFakeConfig());
            var model = new RegisterModel { Username = "existinguser", Password = "Test123!" };

            var result = await controller.Register(model);

            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("Username already exists", conflict.Value.ToString());
        }

        /// <summary>
        /// Test case to verify the behavior when a user logs in with correct credentials.
        /// The test expects a successful login (OkObjectResult).
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreCorrect()
        {
            using var context = new ApplicationDbContext(_dbOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var hashed = BCrypt.Net.BCrypt.HashPassword("Test123!");
            context.Users.Add(new User { Username = "validuser", PasswordHash = hashed, Role = "User" });
            await context.SaveChangesAsync();

            var controller = new AuthController(context, GetFakeConfig());
            var model = new LoginModel { Username = "validuser", Password = "Test123!" };

            var result = await controller.Login(model);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Login successful", okResult.Value.ToString());
        }

        /// <summary>
        /// Test case to verify the behavior when the password is incorrect during login.
        /// The test expects an UnauthorizedObjectResult with an "Invalid username or password" message.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenPasswordIsIncorrect()
        {
            using var context = new ApplicationDbContext(_dbOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var hashed = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");
            context.Users.Add(new User { Username = "user1", PasswordHash = hashed, Role = "User" });
            await context.SaveChangesAsync();

            var controller = new AuthController(context, GetFakeConfig());
            var model = new LoginModel { Username = "user1", Password = "WrongPassword" };

            var result = await controller.Login(model);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Contains("Invalid username or password", unauthorized.Value.ToString());
        }

        /// <summary>
        /// Test case to verify the behavior when the user does not exist during login.
        /// The test expects an UnauthorizedObjectResult with an "Invalid username or password" message.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenUserDoesNotExist()
        {
            using var context = new ApplicationDbContext(_dbOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var controller = new AuthController(context, GetFakeConfig());
            var model = new LoginModel { Username = "ghost", Password = "DoesNotMatter" };

            var result = await controller.Login(model);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Contains("Invalid username or password", unauthorized.Value.ToString());
        }
    }
}
