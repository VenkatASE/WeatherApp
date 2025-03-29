using Microsoft.EntityFrameworkCore;
using WeatherApp.Models;

namespace WeatherApp.Database
{
    public class WeatherDbContext : DbContext
    {
        public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Weather> Weathers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Set default collation for the entire database
            modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configure Weather entity
            modelBuilder.Entity<Weather>(entity =>
            {
                // Set collation for specific columns, if needed
                entity.Property(w => w.CityName)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                // Configure precision for decimal properties
                entity.Property(w => w.Temperature)
                    .HasPrecision(18, 2);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
