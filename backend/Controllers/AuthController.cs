using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
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

    // ================= ME =================
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return NotFound();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.Role
        });
    }

    // ================= REGISTER =================
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (req == null) return BadRequest();

        if (string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.Password) ||
            string.IsNullOrWhiteSpace(req.Username))
            return BadRequest(new { message = "All fields required" });

        var email = req.Email.Trim().ToLower();

        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == email))
            return Conflict(new { message = "Email already used" });

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = req.Username.Trim(),
            Email = email,
            PasswordHash = PasswordService.HashPassword(req.Password),
            Role = "standard",
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(GenerateAuth(user));
    }

    // ================= LOGIN =================
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (req == null) return BadRequest();

        var login = req.Login?.Trim().ToLower();

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Email.ToLower() == login || u.Username.ToLower() == login);

        if (user == null ||
            !PasswordService.VerifyPassword(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials" });

        return Ok(GenerateAuth(user));
    }

    // ================= FORGOT PASSWORD =================
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Email))
            return BadRequest();

        var email = req.Email.Trim().ToLower();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user == null)
            return Ok(new { message = "If email exists, reset link sent" });

        user.ResetToken = Guid.NewGuid().ToString("N");
        user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);

        await _db.SaveChangesAsync();

        var frontendUrl = _config["Frontend:Url"] ?? "http://localhost:5173";
        var resetLink = $"{frontendUrl}/reset-password?token={user.ResetToken}";

        // DEV MODE (туннель)
        Console.WriteLine($"RESET LINK: {resetLink}");

        return Ok(new
        {
            message = "If email exists, reset link sent",
            link = resetLink // можно убрать позже
        });
    }

    // ================= RESET PASSWORD =================
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        if (req == null ||
            string.IsNullOrWhiteSpace(req.Token) ||
            string.IsNullOrWhiteSpace(req.NewPassword))
            return BadRequest();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.ResetToken == req.Token);

        if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
            return BadRequest(new { message = "Token invalid or expired" });

        user.PasswordHash = PasswordService.HashPassword(req.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Password changed successfully" });
    }

    // ================= JWT =================
    private object GenerateAuth(User user)
    {
        return new
        {
            Token = GenerateJwt(user),
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        };
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"] ??
            "SuperSecretKey12345SuperSecretKey12345"));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
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
}