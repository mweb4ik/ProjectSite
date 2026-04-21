using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PcComponentsApi.Data;
using PcComponentsApi.Models;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;
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
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password) || string.IsNullOrWhiteSpace(req.Username))
            return BadRequest(new { message = "Все поля обязательны" });

        if (req.Password.Length < 6)
            return BadRequest(new { message = "Пароль должен быть не менее 6 символов" });

        if (req.Username.Length < 4)
            return BadRequest(new { message = "Имя пользователя должно быть не менее 4 символов" });

        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == req.Email.ToLower()))
            return Conflict(new { message = "Пользователь с таким email уже существует" });

        if (await _db.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict(new { message = "Пользователь с таким именем пользователя уже существует" });

        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = PasswordService.HashPassword(req.Password),
            Role = "standard",
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            Token = GenerateJwt(user),
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Email.ToLower() == req.Login.ToLower() || u.Username == req.Login);

        if (user == null || !PasswordService.VerifyPassword(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Неверный email или пароль" });

        return Ok(new AuthResponse
        {
            Token = GenerateJwt(user),
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        var email = req.Email?.Trim().ToLower();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user == null) return Ok(); // Не показ, что пользователя нет

        user.ResetToken = Guid.NewGuid().ToString();
        user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);
        await _db.SaveChangesAsync();

        try
        {
            var emailSection = _config.GetSection("Email");
            var smtpEmail = emailSection["Address"] ?? throw new Exception("Email не задан");
            var smtpPassword = emailSection["Password"] ?? throw new Exception("Password не задан");
            var host = emailSection["Host"] ?? "smtp.gmail.com";
            var port = int.Parse(emailSection["Port"] ?? "587");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("PcArchitekture", smtpEmail));
            message.To.Add(new MailboxAddress(user.Username, user.Email));
            message.Subject = "Сброс пароля";
            message.Body = new TextPart("plain") {Text = $"https://pc-components-app.vercel.app/reset-password?token={user.ResetToken}"};

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpEmail, smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }

        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.ResetToken == req.Token);

        if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
            return BadRequest(new { message = "Неверный или просроченный токен" });

        user.PasswordHash = PasswordService.HashPassword(req.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        await _db.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            Token = GenerateJwt(user),
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        });
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "SuperSecretKey12345SuperSecretKey12345"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

[HttpGet("me")]
[Authorize]
public async Task<IActionResult> Me()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId))
        return Unauthorized();

    var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null)
        return NotFound();

    return Ok(new
    {
        Username = user.Username,
        Email = user.Email,
        Role = user.Role
    });
}
[HttpPut("update-profile")]
[Authorize]
public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        return Unauthorized(new { message = "Неверный токен" });
    }

    var user = await _context.Users.FindAsync(userId.ToString());
    if (user == null)
    {
        return NotFound(new { message = "Пользователь не найден" });
    }

    if (!string.IsNullOrWhiteSpace(model.Username))
    {
        var exists = await _context.Users.AnyAsync(u => u.Username == model.Username && u.Id != user.Id);
        if (exists)
        {
            return Conflict(new { message = "Такой ник уже занят" });
        }
        user.Username = model.Username;
    }

    if (!string.IsNullOrWhiteSpace(model.NewPassword))
    {
        if (model.NewPassword.Length < 6)
        {
            return BadRequest(new { message = "Пароль должен быть минимум 6 символов" });
        }
        user.PasswordHash = PasswordService.HashPassword(model.NewPassword);
    }

    await _context.SaveChangesAsync();

    return Ok(new 
    { 
        message = "Профиль обновлен",
        username = user.Username,
        role = user.Role 
    });
}
}