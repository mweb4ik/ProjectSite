using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

    // ================= REFRESH TOKEN =================
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        if (req?.RefreshToken == null)
            return BadRequest(new { message = "Refresh token required" });


        var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == req.RefreshToken);


        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Unauthorized(new { message = "Invalid refresh token" });

        // Ротация: старый токен больше не валиден
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        var result = GenerateAuth(user); 

        await _db.SaveChangesAsync();

        return Ok(result);
    }

    // ================= LOGOUT (костыль-отзыв) =================
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _db.Users.FindAsync(userId);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Logged out" });
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



        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        await _db.SaveChangesAsync();

        var frontendUrl = _config["Frontend:Url"] ?? "http://localhost:5173";
        var resetLink = $"{frontendUrl}/reset-password?token={user.ResetToken}";

        Console.WriteLine($"RESET LINK: {resetLink}");

        return Ok(new
        {
            message = "If email exists, reset link sent",
            link = resetLink
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
        
        // 🔥 Костыль: при смене пароля — отзываем рефреш-токены
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Password changed successfully" });
    }

    // ================= JWT HELPERS =================
    
    private string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private object GenerateAuth(User user)
    {
        var refreshToken = GenerateRefreshToken();
        

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(1); 
        _db.SaveChanges(); 

        return new
        {
            AccessToken = GenerateJwt(user, expiresInMinutes: 15), 
            RefreshToken = refreshToken,                            
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            ExpiresIn = 900 // секунд
        };
    }

    private string GenerateJwt(User user, int expiresInMinutes = 15)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "SuperSecretKey12345SuperSecretKey12345"));

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
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}