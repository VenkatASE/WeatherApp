using Xunit; // xUnit namespace
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using WeatherApp.Controllers;
using WeatherApp.Database;
using WeatherApp.Models;
using WeatherApp.DTOs;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace WeatherApp.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly IConfiguration _configuration;
        private readonly WeatherDbContext _context;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            // Mock IConfiguration with necessary values
            _configuration = Substitute.For<IConfiguration>();
            _configuration["Jwt:Key"].Returns("ThisIsASecretKeyThatIsLongEnough");
            _configuration["Jwt:Issuer"].Returns("TestIssuer");
            _configuration["Jwt:Audience"].Returns("TestAudience");

            // Configure DbContext with an in-memory database
            var options = new DbContextOptionsBuilder<WeatherDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDB")
                .Options;
            _context = new WeatherDbContext(options);

            // Seed data for testing
            _context.Users.Add(new User
            {
                UserId = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("testpass"),
                Role = "User"
            });
            _context.SaveChanges();

            // Create AuthController instance
            _authController = new AuthController(_configuration, _context);
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "testpass"
            };

            // Act
            var response = await _authController.Login(request);

            // Assert
            var result = Assert.IsType<OkObjectResult>(response); // Check if the response is 200 OK
            var responseBody = Assert.IsType<BaseResponse<object>>(result.Value); // Validate the response structure
            Assert.True(responseBody.Success);
            Assert.NotNull(responseBody.Data);

            // Check for token in the response
            var tokenData = JsonSerializer.Serialize(responseBody.Data);
            Assert.Contains("Token", tokenData);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "invaliduser",
                Password = "wrongpass"
            };

            // Act
            var response = await _authController.Login(request);

            // Assert
            var result = Assert.IsType<UnauthorizedObjectResult>(response); // Check if the response is 401 Unauthorized
            var responseBody = Assert.IsType<BaseResponse<string>>(result.Value);
            Assert.False(responseBody.Success);
            Assert.Equal("Invalid username or password.", responseBody.Message);
        }

        [Fact]
        public async Task Register_ShouldAddUser_WhenUsernameIsUnique()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "newuser",
                Password = "newpassword"
            };

            // Act
            var response = await _authController.Register(request);

            // Assert
            var result = Assert.IsType<OkObjectResult>(response); // Check if the response is 200 OK
            var responseBody = Assert.IsType<BaseResponse<string>>(result.Value);
            Assert.True(responseBody.Success);
            Assert.Equal("User registered successfully.", responseBody.Message);

            // Verify user is added to the database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
            Assert.NotNull(user);
            Assert.True(BCrypt.Net.BCrypt.Verify("newpassword", user.PasswordHash));
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenUsernameAlreadyExists()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser", // Username already exists
                Password = "testpass"
            };

            // Act
            var response = await _authController.Register(request);

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(response); // Check if the response is 400 Bad Request
            var responseBody = Assert.IsType<BaseResponse<string>>(result.Value);
            Assert.False(responseBody.Success);
            Assert.Equal("Username already exists.", responseBody.Message);
        }
    }
}
