using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PcComponentsApi.Data;
using PcComponentsApi.Models;
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

     // ================= ME =================
    [HttpGet("me")]
    [Authorize] 
    public async Task<IActionResult> GetMe()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

      var user = await _db.Users.FindAsync(userIdClaim.Value);
        if (user == null) return NotFound();

        return Ok(new
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        });
    }
    // ================= REGISTER =================
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (req == null)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.Password) ||
            string.IsNullOrWhiteSpace(req.Username))
            return BadRequest(new { message = "Все поля обязательны" });

        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { message = "Email уже используется" });

        if (await _db.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict(new { message = "Username уже используется" });

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

    // ================= LOGIN =================
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
       var login = req.Login.Trim().ToLower();
var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == login || u.Username.ToLower() == login);
if (user == null || !PasswordService.VerifyPassword(req.Password, user.PasswordHash))
    return Unauthorized(new { message = "Неверный логин или пароль" });
        return Ok(new AuthResponse
        {
            Token = GenerateJwt(user),
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        });
    }

    // ================= FORGOT PASSWORD (БЕЗ SMTP СБОЕВ) =================
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Email))
            return BadRequest();

        var email = req.Email.Trim().ToLower();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user == null)
            return Ok(); 

        user.ResetToken = Guid.NewGuid().ToString();
        user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Reset token created",
            token = user.ResetToken
        });
    }

    // ================= RESET PASSWORD =================
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        if (req == null)
            return BadRequest();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.ResetToken == req.Token);

        if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
            return BadRequest(new { message = "Token invalid or expired" });

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
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "SuperSecretKey12345SuperSecretKey12345"));
    
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
}