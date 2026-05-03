using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcComponentsApi.Data;
using PcComponentsApi.Models;

[ApiController]
[Route("api/[controller]")]
public class BiosController : ControllerBase {
      private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public BiosController(AppDbContext db, IConfiguration config)
    {
        _db = db;
    }
[HttpGet]//Получить BIOS
public async Task<IActionResult> GetAll()
{
   return Ok(await _db.BiosVersions
    .OrderByDescending(x => x.ReleaseDate)
    .ToListAsync());
}
//Проверка CPU = BIOS
[HttpPost("check-cpu")]
public async Task<IActionResult> CheckCpu([FromBody] CpuCheckRequest req)
{
    var support = await _db.CpuSupports
        .FirstOrDefaultAsync(x => x.CpuId == req.CpuId && x.BiosVersionId == req.BiosId);

    if (support == null)
        return NotFound();

    return Ok(support.IsSupported);
}
//Проверка обноовления
[HttpPost("check-update")]
public async Task<IActionResult> CheckUpdate([FromBody] UpdateCheckRequest req)
{
    var current = await _db.BiosVersions
        .FirstOrDefaultAsync(x => x.Version == req.CurrentVersion);

    var target = await _db.BiosVersions
        .FirstOrDefaultAsync(x => x.Version == req.TargetVersion);

    if (current == null || target == null)
        return NotFound("Версия BIOS не найдена");

    // Простейшая логика риска
    double stabilityDiff = target.Stability - current.Stability;

    string risk = stabilityDiff >= 5 ? "LOW"
               : stabilityDiff >= 0 ? "MEDIUM"
               : "HIGH";

    return Ok(new
    {
        risk,
        current = current.Version,
        target = target.Version,
        stabilityChange = stabilityDiff
    });
}
}