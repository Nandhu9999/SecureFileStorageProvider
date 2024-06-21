using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecureFileStorageProvider.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SecureFileStorageProvider.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDTO request)
        {
            try
            {
                string email = request.Email;
                string password = request.Password;

                var user = await _context.Users.FirstOrDefaultAsync( u => u.Email == email );
                if (user == null) return Unauthorized(new { Success = false, Error = "User not found." });
                if ( !isPasswordVerified(password, user.Password) ) return Unauthorized(new { Success = false, Error = "User not found." });

                var USER_AUTH_CLAIMS = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                };
                var AUTH_SIGNING_KEY = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
                var SIGNING_CREDENTIALS = new SigningCredentials(AUTH_SIGNING_KEY, SecurityAlgorithms.HmacSha256);
                var TOKEN = new JwtSecurityToken(
                    issuer: _config["JWT:Issuer"],
                    audience: _config["JWT:Audience"],
                    expires: DateTime.Now.AddDays(1),
                    claims: USER_AUTH_CLAIMS,
                    signingCredentials: SIGNING_CREDENTIALS
                );
                var UserResponse = new { UserId = user.UserId, Username = user.Username, Email = user.Email };
                return Ok(new 
                { 
                    Success = true, 
                    User = UserResponse, 
                    accessToken = new JwtSecurityTokenHandler().WriteToken(TOKEN), 
                    expiration = TOKEN.ValidTo,
                    tokenType = "Bearer"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDTO request)
        {
            try
            {
                string email = request.Email;
                string password = hashPassword(request.Password);
                string username = email.Split("@")[0].ToUpper();

                var newUser = new User
                {
                    Username = username,
                    Email = email,
                    Password = password,
                };

                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true });
            } catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Error = ex.Message });
            }
        }
        public class UserDTO
        {
            public string Email { get; set; } = null!;
            public string Password { get; set; } = null!;
        }
        private static bool isPasswordVerified(string inputPass, string storedPass)
        {
            if (string.IsNullOrEmpty(inputPass)) return false;
            if (string.IsNullOrEmpty(storedPass)) return false;
            var hashedInputPass = hashPassword(inputPass);
            return hashedInputPass.Equals(storedPass, StringComparison.OrdinalIgnoreCase);
        }
        private static string hashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
