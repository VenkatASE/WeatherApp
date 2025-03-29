using Microsoft.EntityFrameworkCore;
using WeatherApp.Database;
using WeatherApp.Models;

namespace WeatherApp.Services
{

    public interface IWeatherService
    {
        Task<IEnumerable<Weather>> GetAllWeatherAsync();
        Task<Weather?> GetWeatherByIdAsync(Guid id);
        Task<Weather> AddWeatherAsync(Weather weather);
        Task<Weather> UpdateWeatherAsync(Guid id, Weather weather);
        Task<bool> DeleteWeatherAsync(Guid id);
    }
    public class WeatherService : IWeatherService
    {
        private readonly WeatherDbContext _context;

        public WeatherService(WeatherDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Weather>> GetAllWeatherAsync()
        {
            return await _context.Weathers.ToListAsync();
        }

        public async Task<Weather?> GetWeatherByIdAsync(Guid id)
        {
            return await _context.Weathers.FindAsync(id);
        }

        public async Task<Weather> AddWeatherAsync(Weather weather)
        {
            weather.LastUpdated = DateTime.UtcNow;
            _context.Weathers.Add(weather);
            await _context.SaveChangesAsync();
            return weather;
        }

        public async Task<Weather> UpdateWeatherAsync(Guid id, Weather updatedWeather)
        {
            var existingWeather = await _context.Weathers.FindAsync(id);
            if (existingWeather != null)
            {
                existingWeather.CityName = updatedWeather.CityName;                
                existingWeather.Temperature = updatedWeather.Temperature;
                existingWeather.WeatherCondition = updatedWeather.WeatherCondition;
                existingWeather.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return existingWeather;
            }
            throw new KeyNotFoundException("Weather record not found.");
        }

        public async Task<bool> DeleteWeatherAsync(Guid id)
        {
            var weather = await _context.Weathers.FindAsync(id);
            if (weather != null)
            {
                _context.Weathers.Remove(weather);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
