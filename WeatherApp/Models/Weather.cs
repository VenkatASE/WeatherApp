using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace WeatherApp.Models
{
    public class Weather
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string CityName { get; set; } = string.Empty; // Matches the document schema

        [Required]
        public decimal Temperature { get; set; } // Use decimal for precision in storing temperatures

        [Required]
        public string WeatherCondition { get; set; } = string.Empty; // Description of weather (e.g., Clear sky, Rain)

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow; // Timestamp of last data update

      
    }
}
