using System;
using System.Net;
using System.Threading.Tasks;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILoginLogger _loginLogger;
        private readonly IConfiguration _config;

        public AuthController(ILoginLogger loginLogger, IConfiguration config)
        {
            _loginLogger = loginLogger;
            _config = config;
        }

        public class LoginRequest
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var username = req?.Username?.Trim() ?? string.Empty;
            var password = req?.Password ?? string.Empty;

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers["User-Agent"].ToString();

            var success = username == "admin" && password == "admin";

            await _loginLogger.LogAsync(username, success, ip, ua);

            if (!success)
            {
                return Unauthorized(new { message = "用户名或密码错误" });
            }

            var token = GenerateJwtToken(username);
            return Ok(new { token });
        }

        private string GenerateJwtToken(string username)
        {
            var issuer = _config["Jwt:Issuer"] ?? "login-system";
            var audience = _config["Jwt:Audience"] ?? issuer;
            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
            var expiresHours = int.TryParse(_config["Jwt:ExpiresHours"], out var h) ? h : 2;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiresHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
