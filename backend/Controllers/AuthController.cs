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
using Microsoft.AspNetCore.Authorization;
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
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password)  || string.IsNullOrWhiteSpace(req.Username))
           return BadRequest(new { message = "Все поля обязательны" });

        if (req.Password.Length < 6)
            return BadRequest(new { message = "Пароль должен быть не менее 6 символов" });
         if (req.Username.Length <4)
            return BadRequest(new { message = "Имя пользователя должно быть не менее 4 символов" });

        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
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
        var token = GenerateJwt(user);
        return Ok(new AuthResponse { Token = token,
        Username = user.Username,
         Email = user.Email,
         Role = user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Login || u.Username == req.Login);
        if (user == null)
            return Unauthorized(new { message = "Неверный email или пароль" });

        if (!PasswordService.VerifyPassword(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Неверный email или пароль" });
        var token = GenerateJwt(user);
        return Ok(new AuthResponse {
        Token = token,
        Username = user.Username,
        Email = user.Email,
         Role = user.Role });
    }
[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
{
    var email = req.Email?.Trim().ToLower();

    var user = await _db.Users
        .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

    if (user == null)
        return Ok();

    var resetToken = Guid.NewGuid().ToString();

    user.ResetToken = resetToken;
    user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);

    await _db.SaveChangesAsync();

    try
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Pc-Architekture website", "kiseluk780@gmail.com"));
        emailMessage.To.Add(new MailboxAddress(user.Username, user.Email));
        emailMessage.Subject = "Сброс пароля";
        emailMessage.Body = new TextPart("plain")
        {
            Text = $"http://localhost:5173/reset-password?token={resetToken}"
        };

        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync("kiseluk780@gmail.com", "tgndfnuxzcdrodsk");
        await client.SendAsync(emailMessage);
        await client.DisconnectAsync(true);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
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

    var jwt = GenerateJwt(user);

    return Ok(new AuthResponse
    {
        Token = jwt,
        Username = user.Username,
        Email = user.Email,
        Role = user.Role
    });
}
private string GenerateJwt(User user)
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "SuperSecretKey12345SuperSecretKey12345"));

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
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
[HttpGet("me")]
[Authorize]
public IActionResult Me()
{
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var user = _db.Users.Find(userId);
if (user == null)
    return NotFound();
        return Ok(new{
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        });
}
}
