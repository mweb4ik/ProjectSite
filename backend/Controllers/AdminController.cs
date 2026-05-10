using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcComponentsApi.Data;
using PcComponentsApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace PcComponentsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/admin/users - получить всех пользователей
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _db.Users
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.Role,
                u.CreatedAt
            })
            .ToListAsync();
        
        return Ok(users);
    }

    // PUT: api/admin/users/{id}/role - изменить роль пользователя
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> ChangeUserRole(string id, [FromBody] RoleChangeRequest request)
    {
        if (string.IsNullOrEmpty(request.Role))
            return BadRequest(new { message = "Роль не указана" });

        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        // Защита от удаления последней админской роли
        if (user.Role == "admin" && request.Role != "admin")
        {
            var adminCount = await _db.Users.CountAsync(u => u.Role == "admin");
            if (adminCount <= 1)
                return BadRequest(new { message = "Нельзя понизить последнего администратора" });
        }

        user.Role = request.Role;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Роль изменена", user.Id, user.Role });
    }

    // DELETE: api/admin/users/{id} - удалить пользователя
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        // Защита от удаления последнего админа
        if (user.Role == "admin")
        {
            var adminCount = await _db.Users.CountAsync(u => u.Role == "admin");
            if (adminCount <= 1)
                return BadRequest(new { message = "Нельзя удалить последнего администратора" });
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Пользователь удалён" });
    }

    // GET: api/admin/stats - статистика для админки
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = new
        {
            totalUsers = await _db.Users.CountAsync(),
            totalComponents = await _db.Components.CountAsync(),
            totalBuilds = await _db.PCBuilds.CountAsync(),
            totalQuizResults = await _db.QuizResults.CountAsync()
        };

        return Ok(stats);
    }
}

public class RoleChangeRequest
{
    public string Role { get; set; } = string.Empty;
}
