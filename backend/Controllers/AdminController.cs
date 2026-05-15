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

    

    // DTO для пользователя
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // GET: api/admin/users?page=1&limit=5
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int page = 1, 
        [FromQuery] int limit = 5)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 50);

        var query = _db.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            })
            .OrderByDescending(u => u.CreatedAt);

        var totalCount = await query.CountAsync();
        
        var data = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return Ok(new PaginatedResult<UserDto>
        {
            Data = data,
            Page = page,
            PageSize = limit,
            TotalCount = totalCount
        });
    }

    // GET: api/admin/components?page=1&limit=5
    [HttpGet("components")]
    public async Task<IActionResult> GetComponents(
        [FromQuery] int page = 1, 
        [FromQuery] int limit = 5)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 50);

        var query = _db.Components
            .OrderBy(c => c.Name);

        var totalCount = await query.CountAsync();
        
        var data = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        //  Явное приведение Component -> object через Cast
        return Ok(new PaginatedResult<object>
        {
            Data = data.Cast<object>().ToList(),
            Page = page,
            PageSize = limit,
            TotalCount = totalCount
        });
    }

    //  PUT: api/admin/users/{id}/role
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> ChangeUserRole(string id, [FromBody] RoleChangeRequest request)
    {
        if (string.IsNullOrEmpty(request.Role))
            return BadRequest(new { message = "Роль не указана" });

        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

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

    //  DELETE: api/admin/users/{id}
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

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

    //  GET: api/admin/stats
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