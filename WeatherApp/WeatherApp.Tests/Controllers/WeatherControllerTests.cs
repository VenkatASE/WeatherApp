using Xunit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using WeatherApp.Controllers;
using WeatherApp.Database;
using WeatherApp.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WeatherApp.Tests.Controllers
{
    public class WeatherControllerTests
    {
        private readonly WeatherDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly WeatherController _weatherController;

        public WeatherControllerTests()
        {
            // Substitute IConfiguration
            _configuration = Substitute.For<IConfiguration>();

            // Configure DbContext with in-memory database
            var options = new DbContextOptionsBuilder<WeatherDbContext>()
                .UseInMemoryDatabase(databaseName: "WeatherTestDB")
                .Options;
            _context = new WeatherDbContext(options);

            // Seed data for testing
            _context.Weathers.AddRange(new List<Weather>
            {
                new Weather { CityName = "City1", Temperature = 25, WeatherCondition = "Sunny" },
                new Weather { CityName = "City2", Temperature = 18, WeatherCondition = "Cloudy" }
            });
            _context.SaveChanges();

            // Create WeatherController instance
            _weatherController = new WeatherController(_context, _configuration, new System.Net.Http.HttpClient());
        }

        [Fact]
        public async Task GetAllWeather_ShouldReturnAllWeatherData()
        {
            // Act
            var result = await _weatherController.GetAllWeather();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var weatherData = Assert.IsType<List<Weather>>(okResult.Value);
            Assert.Equal(2, weatherData.Count);
        }

        [Fact]
        public async Task GetWeatherByCity_ShouldReturnWeather_WhenCityExists()
        {
            // Act
            var result = await _weatherController.GetWeatherByCity("City1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var weather = Assert.IsType<Weather>(okResult.Value);
            Assert.Equal("City1", weather.CityName);
        }

        [Fact]
        public async Task GetWeatherByCity_ShouldReturnNotFound_WhenCityDoesNotExist()
        {
            // Act
            var result = await _weatherController.GetWeatherByCity("NonExistentCity");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task AddWeather_ShouldAddWeatherData()
        {
            // Arrange
            var cityName = "NewCity";

            // Act
            var result = await _weatherController.AddWeather(cityName);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(cityName, ((Weather)createdResult.Value).CityName);
        }

        [Fact]
        public async Task UpdateWeather_ShouldUpdateWeatherData_WhenCityExists()
        {
            // Arrange
            var cityName = "City1";

            // Act
            var result = await _weatherController.UpdateWeather(cityName);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteWeather_ShouldRemoveWeatherData_WhenCityExists()
        {
            // Arrange
            var cityName = "City1";

            // Act
            var result = await _weatherController.DeleteWeather(cityName);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
