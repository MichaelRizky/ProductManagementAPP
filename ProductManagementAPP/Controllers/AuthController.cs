using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductManagementAPP.Data;
using ProductManagementAPP.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProductManagementAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger; // <-- Tambahkan ILogger

        public AuthController(AppDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserAuthDto request)
        {
            _logger.LogInformation($"Registration attempt for username: {request.Username}");

            try
            {
                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                {
                    _logger.LogWarning($"Registration failed. Username '{request.Username}' already exists.");
                    return BadRequest(new { Message = "Username already exists." });
                }

                // Hash password using BCrypt
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var user = new User
                {
                    Username = request.Username,
                    PasswordHash = passwordHash
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User '{request.Username}' registered successfully.");
                return Ok(new { Message = "User registered successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred during registration for username: {request.Username}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserAuthDto request)
        {
            _logger.LogInformation($"Login attempt for username: {request.Username}");

            try
            {
                // Find user in database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
                if (user == null)
                {
                    _logger.LogWarning($"Login failed. Invalid username: {request.Username}");
                    return Unauthorized(new { Message = "Invalid username or password." });
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning($"Login failed. Invalid password for username: {request.Username}");
                    return Unauthorized(new { Message = "Invalid username or password." });
                }

                // Generate JWT Token
                var token = GenerateJwtToken(user);

                _logger.LogInformation($"User '{request.Username}' logged in successfully.");
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred during login for username: {request.Username}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["Key"]!));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), // Token expires in 2 hours
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}