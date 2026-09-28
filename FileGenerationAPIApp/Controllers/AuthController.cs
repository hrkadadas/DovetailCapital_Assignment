using Microsoft.AspNetCore.Mvc;
using FileGenerationAPIApp.Data;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using FileGenerationAPIApp.Models;

namespace FileGenerationAPIApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FileGenerationAPIApp.Wrappers.IWrapper _wrapper;
        private readonly IConfiguration _config;

        public AuthController(FileGenerationAPIApp.Wrappers.IWrapper wrapper, IConfiguration config, ILogger<AuthController> logger)
        {
            _wrapper = wrapper;
            _config = config;
            _logger = logger;
        }

        private readonly ILogger<AuthController> _logger;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.Password))
                    throw new ArgumentException("Password cannot be empty.");
                if (string.IsNullOrWhiteSpace(req.Username))
                    throw new ArgumentException("Username cannot be empty.");

                var user = await _wrapper.Users.FindByUsernameAndPasswordAsync(req.Username, req.Password);
                if (user == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid credentials" });

                var jwt = _config.GetSection("Jwt");
                var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwt.GetValue<string>("Key") ?? "dev_key"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role?.Name ?? "User")
                };

                var token = new JwtSecurityToken(
                    issuer: jwt.GetValue<string>("Issuer"),
                    audience: jwt.GetValue<string>("Audience"),
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(jwt.GetValue<int>("ExpiresMinutes")),
                    signingCredentials: creds
                );

                return Ok(new ApiResponse { Success = true, Data = new { token = new JwtSecurityTokenHandler().WriteToken(token) } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {User}", req?.Username);
                await _wrapper.ExceptionLogs.AddAsync(new AppExceptionLog { Message = ex.Message, StackTrace = ex.StackTrace, Path = HttpContext.Request.Path, Level = "Error", UserId = req?.Username });
                await _wrapper.ExceptionLogs.SaveChangesAsync();
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }
    }

    public record LoginRequest(string Username, string Password);
}
