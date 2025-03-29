using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

using WeatherApp.Database;
using WeatherApp.Models;

namespace WeatherApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WeatherController : ControllerBase
    {
        private readonly WeatherDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public WeatherController(WeatherDbContext context, IConfiguration configuration, HttpClient httpClient)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllWeather()
        {
            var weatherData = await _context.Weathers.ToListAsync();
            return Ok(weatherData);
        }

        [HttpGet("{city}")]
        public async Task<IActionResult> GetWeatherByCity(string city)
        {
            var weather = await _context.Weathers.FirstOrDefaultAsync(w => w.CityName.ToLower() == city.ToLower());
            if (weather == null)
            {
                return NotFound("Weather data not found.");
            }
            return Ok(weather);
        }

        [HttpPost]
        public async Task<IActionResult> AddWeather([FromQuery] string cityName)
        {
            var weatherData = await FetchWeatherFromApi(cityName);
            if (weatherData == null)
            {
                return BadRequest("Unable to fetch weather data.");
            }

            _context.Weathers.Add(weatherData);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetWeatherByCity), new { city = weatherData.CityName }, weatherData);
        }

        [HttpPut("{city}")]
        public async Task<IActionResult> UpdateWeather(string city)
        {
            var existingWeather = await _context.Weathers.FirstOrDefaultAsync(w => w.CityName.ToLower() == city.ToLower());
            if (existingWeather == null)
            {
                return NotFound("Weather data not found.");
            }

            var updatedWeather = await FetchWeatherFromApi(city);
            if (updatedWeather == null)
            {
                return BadRequest("Unable to fetch weather data.");
            }

            existingWeather.Temperature = updatedWeather.Temperature;
            existingWeather.WeatherCondition = updatedWeather.WeatherCondition;
            existingWeather.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{city}")]
        public async Task<IActionResult> DeleteWeather(string city)
        {
            var weather = await _context.Weathers.FirstOrDefaultAsync(w => w.CityName.ToLower() == city.ToLower());
            if (weather == null)
            {
                return NotFound("Weather data not found.");
            }

            _context.Weathers.Remove(weather);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<Weather?> FetchWeatherFromApi(string city)
        {
            var apiKey = _configuration["OpenWeather:ApiKey"];
            var apiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<OpenWeatherResponse>(apiUrl);
                if (response == null) return null;

                return new Weather
                {
                    CityName = city,
                    Temperature = response.Main.Temp,
                    WeatherCondition = response.Weather[0].Description,
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch
            {
                return null;
            }
        }
    }

    public class OpenWeatherResponse
    {
        public WeatherInfo[] Weather { get; set; } = Array.Empty<WeatherInfo>();
        public MainInfo Main { get; set; } = new MainInfo();

        public class WeatherInfo
        {
            public string Description { get; set; } = string.Empty;
        }

        public class MainInfo
        {
            public decimal Temp { get; set; }
        }
    }
}
