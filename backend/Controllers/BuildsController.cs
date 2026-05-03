using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcComponentsApi.Data;
using PcComponentsApi.Models;
using System.Security.Claims;

namespace PcComponentsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class BuildsController : ControllerBase
{
    private readonly AppDbContext _db;

    public BuildsController(AppDbContext db)
    {
        _db = db;
    }

    //Получение ID текущего пользователя из токена
    private string GetCurrentUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            userId = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;
        }
        return userId ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    //Список сборок пользователя
    [HttpGet]
    public async Task<IActionResult> GetUserBuilds()
    {
        var userId = GetCurrentUserId();
        var builds = await _db.PCBuilds
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.Id)
            .ToListAsync();

        return Ok(builds);
    }

    //Получить конкретную сборку
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBuildById(string id)
    {
        var userId = GetCurrentUserId();
        var build = await _db.PCBuilds
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if (build == null)
            return NotFound(new { message = "Сборка не найдена или доступ запрещен" });

        return Ok(build);
    }

    //Создать новую сборку
    [HttpPost]
    public async Task<IActionResult> CreateBuild([FromBody] CreateBuildRequest request)
    {
        var userId = GetCurrentUserId();

        var newBuild = new PCBuild
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            ComponentsJson = request.ComponentsJson,
            TotalPrice = request.TotalPrice,
            Currency = request.Currency ?? "USD",
            IsCompatible = request.IsCompatible
        };

        _db.PCBuilds.Add(newBuild);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBuildById), new { id = newBuild.Id }, newBuild);
    }

    //Обновить существующую сборку
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBuild(string id, [FromBody] CreateBuildRequest request)
    {
        var userId = GetCurrentUserId();

        var build = await _db.PCBuilds
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if (build == null)
            return NotFound(new { message = "Сборка не найдена" });

        build.ComponentsJson = request.ComponentsJson;
        build.TotalPrice = request.TotalPrice;
        build.Currency = request.Currency ?? "USD";
        build.IsCompatible = request.IsCompatible;

        await _db.SaveChangesAsync();

        return Ok(build);
    }

    // DELETE: api/builds/{id} - Удалить сборку
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBuild(string id)
    {
        var userId = GetCurrentUserId();

        var build = await _db.PCBuilds
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if (build == null)
            return NotFound(new { message = "Сборка не найдена" });

        _db.PCBuilds.Remove(build);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Сборка удалена" });
    }
}

public class CreateBuildRequest
{
    public string ComponentsJson { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string? Currency { get; set; }
    public bool IsCompatible { get; set; }
}