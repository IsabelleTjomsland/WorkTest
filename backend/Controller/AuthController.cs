using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Worktest.backend.Models;
using BCrypt.Net;

namespace Worktest.backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/auth/register (Registrera användare)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            if (registerModel == null || string.IsNullOrWhiteSpace(registerModel.Username) || string.IsNullOrWhiteSpace(registerModel.Password))
            {
                return BadRequest(new { Message = "Invalid request. Username and password are required." });
            }

            // Kolla om användaren redan finns
            var existingUser = await _context.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.Username == registerModel.Username);

            if (existingUser != null)
            {
                return Conflict(new { Message = "Username already exists. Please log in instead." });
            }

            // Hasha lösenordet
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerModel.Password);

            var newUser = new User
            {
                Username = registerModel.Username,
                PasswordHash = hashedPassword,
                Role = "User"  // Default roll
            };

            try
            {
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An internal server error occurred while creating the user." });
            }

            var token = GenerateJwtToken(newUser);
            return Ok(new { Message = "Registration successful.", Token = token });
        }

        // POST: api/auth/login (Logga in användare)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            if (loginModel == null || string.IsNullOrWhiteSpace(loginModel.Username) || string.IsNullOrWhiteSpace(loginModel.Password))
            {
                return BadRequest(new { Message = "Invalid request. Username and password are required." });
            }

            var user = await _context.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.Username == loginModel.Username);

            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid username or password." });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginModel.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized(new { Message = "Invalid username or password." });
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Message = "Login successful.", Token = token });
        }

        // Helper metod för att skapa JWT-token
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role) // Lägg till användarrollen i token
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
