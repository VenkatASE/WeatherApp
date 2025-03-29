using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace WeatherApp.Database
{
    public class WeatherDbContextFactory : IDesignTimeDbContextFactory<WeatherDbContext>
    {
        public WeatherDbContext CreateDbContext(string[] args)
        {
            // Build configuration to get the connection string
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Ensure this points to your project directory
                .AddJsonFile("appsettings.json")             // Load appsettings.json
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<WeatherDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new WeatherDbContext(optionsBuilder.Options);
        }
    }
}
