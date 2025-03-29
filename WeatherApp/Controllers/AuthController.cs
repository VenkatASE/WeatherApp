using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WeatherApp.DTOs;
using WeatherApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.Data;
using WeatherApp.Database;

namespace WeatherApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly WeatherDbContext _context;

        public AuthController(IConfiguration configuration, WeatherDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] DTOs.LoginRequest loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new DTOs.BaseResponse<string>(
                    success: false,
                    message: "Invalid username or password.",
                    data: null
                ));
            }

            var token = GenerateJwtToken(user);
            return Ok(new BaseResponse<object>(
                success: true,
                message: "Login successful.",
                data: new { Token = token }
            ));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] DTOs.LoginRequest registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return BadRequest(new BaseResponse<string>(
                    success: false,
                    message: "Username already exists."
                ));
            }

            var user = new User
            {
                Username = registerDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = "User" // Default role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<string>(
                success: true,
                message: "User registered successfully."
            ));
        }

        private string GenerateJwtToken(User user)
        {
            var keyString = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString) || Encoding.UTF8.GetBytes(keyString).Length < 32)
            {
                throw new ArgumentException("The JWT key must be at least 256 bits (32 characters) long.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

