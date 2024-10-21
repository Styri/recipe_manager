using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using System.Data;
using RecipeManager.Models;

namespace RecipeManager.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, IDbConnection dbConnection, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _dbConnection = dbConnection;
            _logger = logger;
        }

        private ObjectResult HandleException(Exception ex, string errorMessage, string userMessage)
        {
            _logger.LogError(ex, errorMessage);
            return StatusCode(500, userMessage);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto registrationDto)
        {
            try
            {
                var existingUser = await _dbConnection.QuerySingleOrDefaultAsync<User>(
                    "SELECT * FROM users WHERE username = @Username", new { registrationDto.Username });

                if (existingUser != null)
                {
                    return BadRequest("Username already exists");
                }

                var user = new User
                {
                    Username = registrationDto.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registrationDto.Password)
                };

                var insertQuery = "INSERT INTO users (username, passwordhash) VALUES (@Username, @PasswordHash)";
                await _dbConnection.ExecuteAsync(insertQuery, user);

                return Ok("User registered successfully");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error occurred while registering user", "An error occurred while processing your request");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserRegistrationDto loginDto)
        {
            try
            {
                var user = await _dbConnection.QuerySingleOrDefaultAsync<User>(
                    "SELECT * FROM users WHERE username = @Username", new { loginDto.Username });

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    return Unauthorized("Invalid username or password");
                }

                var token = GenerateJwtToken(user);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error occurred while logging in user", "An error occurred while processing your request");
            }
        }

        private string GenerateJwtToken(User user)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                };

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(45),
                    signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating JWT token", ex);
            }
        }
    }
}