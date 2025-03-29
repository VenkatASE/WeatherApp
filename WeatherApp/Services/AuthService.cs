using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WeatherApp.Database;
using WeatherApp.Models;

namespace WeatherApp.Services
{

    public interface IAuthService
    {
        Task<string> AuthenticateAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string password);
    }
    public class AuthService : IAuthService
    {
        private readonly WeatherDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(WeatherDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return string.Empty;

            // Generate JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, user.Role) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
                return false;

            var newUser = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return true;
        }

        public void SomeMethod(BinaryReader binaryReader)
        {
            // Implementation for SomeMethod
        }

        public void ExampleUsage(string filePath)
        {
            var binaryReader = new BinaryReader(File.OpenRead(filePath));
            SomeMethod(binaryReader);
        }
    }
}
