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

        /// <summary>
        /// Registers a new user by accepting the registration details (username and password), 
        /// checking if the user already exists, hashing the password, 
        /// and saving the user to the database.
        /// </summary>
        /// <param name="registerModel">The model containing the registration information.</param>
        /// <returns>An IActionResult indicating the result of the registration process.</returns>
        // POST: api/auth/register (Registrera användare)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            // Validate the input model
            if (registerModel == null || string.IsNullOrWhiteSpace(registerModel.Username) || string.IsNullOrWhiteSpace(registerModel.Password))
            {
                return BadRequest(new { Message = "Invalid request. Username and password are required." });
            }

            // Check if the user already exists
            var existingUser = await _context.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.Username == registerModel.Username);

            if (existingUser != null)
            {
                return Conflict(new { Message = "Username already exists. Please log in instead." });
            }

            // Hash the password using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerModel.Password);

            var newUser = new User
            {
                Username = registerModel.Username,
                PasswordHash = hashedPassword,
                Role = "User"  // Default role for new users
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

            // Generate JWT token for the newly registered user
            var token = GenerateJwtToken(newUser);
            return Ok(new { Message = "Registration successful.", Token = token });
        }

        /// <summary>
        /// Authenticates a user by verifying the provided username and password, 
        /// and returns a JWT token upon successful login.
        /// </summary>
        /// <param name="loginModel">The model containing the login credentials (username and password).</param>
        /// <returns>An IActionResult indicating the result of the login process.</returns>
        // POST: api/auth/login (Logga in användare)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            // Validate the login credentials
            if (loginModel == null || string.IsNullOrWhiteSpace(loginModel.Username) || string.IsNullOrWhiteSpace(loginModel.Password))
            {
                return BadRequest(new { Message = "Invalid request. Username and password are required." });
            }

            // Retrieve the user from the database by username
            var user = await _context.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.Username == loginModel.Username);

            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid username or password." });
            }

            // Verify the password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginModel.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized(new { Message = "Invalid username or password." });
            }

            // Generate JWT token for the authenticated user
            var token = GenerateJwtToken(user);
            return Ok(new { Message = "Login successful.", Token = token });
        }

        /// <summary>
        /// Helper method to generate a JWT token for a given user.
        /// </summary>
        /// <param name="user">The user for whom the JWT token will be generated.</param>
        /// <returns>A string representing the JWT token.</returns>
        // Helper method for generating a JWT token
        private string GenerateJwtToken(User user)
        {
            // Define the claims that will be included in the token
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role) // Add the user's role to the token
            };

            // Create the signing credentials using the secret key from configuration
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationInMinutes"])),
                signingCredentials: creds
            );

            // Return the token as a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
