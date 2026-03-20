using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PcComponentsApi.Data;
using PcComponentsApi.Models;

namespace PcComponentsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Email и пароль обязательны" });

        if (req.Password.Length < 6)
            return BadRequest(new { message = "Пароль должен быть не менее 6 символов" });

        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { message = "Пользователь с таким email уже существует" });

        var user = new User
        {
            Email = req.Email,
            PasswordHash = PasswordService.HashPassword(req.Password),
            Role = "standard",
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = GenerateJwt(user);
        return Ok(new AuthResponse { Token = token, Email = user.Email, Role = user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user == null)
            return Unauthorized(new { message = "Неверный email или пароль" });

        if (!PasswordService.VerifyPassword(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Неверный email или пароль" });

        var token = GenerateJwt(user);
        return Ok(new AuthResponse { Token = token, Email = user.Email, Role = user.Role });
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "SuperSecretKey12345SuperSecretKey12345"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
