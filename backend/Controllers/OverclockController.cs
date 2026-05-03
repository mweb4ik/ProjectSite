using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcComponentsApi.Data;
using PcComponentsApi.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class OverclockController : ControllerBase {
      private readonly AppDbContext _db;

    public OverclockController(AppDbContext db)
    {
        _db = db;
    }
//Симуляция
[HttpPost("simulate")]
public IActionResult Simulate([FromBody] OverclockRequest req)
{
    double temp = 30 + req.Frequency * 8 + req.Voltage * 20;
    double stability = 100 - (req.Frequency * 5 + req.Voltage * 10);

    return Ok(new
    {   
        temperature = temp,
        stability = stability,
        success = stability > 50
    });
}
//Сохранение профиля
[Authorize]
[HttpPost("save")]
public async Task<IActionResult> Save(OverclockProfile profile)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    profile.UserId = userId!;
    _db.OverclockProfiles.Add(profile);
    await _db.SaveChangesAsync();
    if (userId == null)return Unauthorized();
    return Ok(profile);
}
[HttpGet]//Получить профили
public async Task<IActionResult> GetProfiles()
{
    return Ok(await _db.OverclockProfiles.ToListAsync());
}
}