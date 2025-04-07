using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace Worktest.backend.Services
{
    /// <summary>
    /// A service that handles authentication tasks such as validating users and generating JWT tokens.
    /// </summary>
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to retrieve application settings.</param>
        /// <param name="context">The database context to interact with the user data.</param>
        public AuthService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Validates a user by checking the provided username and password against the stored data.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>Returns true if the user is valid, otherwise false.</returns>
        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user != null && VerifyPasswordHash(password, user.PasswordHash))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Generates a JWT token for the specified username.
        /// </summary>
        /// <param name="username">The username for which the JWT token will be generated.</param>
        /// <returns>The generated JWT token as a string.</returns>
        public string GenerateJwtToken(string username)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(jwtSettings["ExpirationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Verifies if the provided password matches the stored password hash.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="storedHash">The stored password hash to compare with.</param>
        /// <returns>Returns true if the password is valid, otherwise false.</returns>
        private bool VerifyPasswordHash(string password, string storedHash)
        {
            // Here you should use proper password hashing (e.g., bcrypt or PBKDF2)
            return password == storedHash; // NOTE: This is simplified for example purposes!
        }
    }
}
