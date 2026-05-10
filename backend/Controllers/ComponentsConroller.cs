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
public class ComponentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ComponentsController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/components?name=rtx
    [HttpGet]
    public async Task<IActionResult> GetComponentsByName([FromQuery] string? name)
    {
        var query = _db.Components.AsQueryable();
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(c => c.Name.Contains(name));
        }
        var components = await query.ToListAsync();
        return Ok(components);
    }

    // GET: api/components/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetComponentById(string id)
    {
        var component = await _db.Components.FindAsync(id);
        if (component == null)
            return NotFound(new { message = "Компонент не найден!" });

        return Ok(new
        {
            Id = component.Id,
            Name = component.Name,
            Category = component.Category.ToString(),
            Price = component.Price,
            Currency = component.Currency,
            Specifications = component.Specifications,
            Socket = component.Socket,
            PowerConsumption = component.PowerConsumption,
            ImageUrl = component.ImageUrl
        });
    }

    // GET: api/components/categories?category=videocard
    [HttpGet("categories")]
    public async Task<IActionResult> GetComponentsByCategory([FromQuery] string? category)
    {
        var query = _db.Components.AsQueryable();
        if (!string.IsNullOrEmpty(category))
        {
            if (Enum.TryParse<ComponentCategory>(category, ignoreCase: true, out var parsedCategory))
            {
                query = query.Where(c => c.Category == parsedCategory);
            }
            else
            {
                return BadRequest(new { message = "Неверный тип категории" });
            }
        }
        var components = await query.ToListAsync();
        return Ok(components);
    }

    // POST: api/components/check-compatibility
    // проверка совместимости
    [HttpPost("check-compatibility")]
    public async Task<IActionResult> CheckCompatibility([FromBody] BuildRequest request)
    {
        if (request == null || request.ComponentIds == null || !request.ComponentIds.Any())
            return BadRequest(new { message = "Список компонентов пуст" });

        var components = await _db.Components
            .Where(c => request.ComponentIds.Contains(c.Id))
            .ToListAsync();

        if (components.Count != request.ComponentIds.Count)
            return NotFound(new { message = "Некоторые компоненты не найдены в базе" });

        var errors = new List<string>();
        var warnings = new List<string>();

        // поиск компонентов по категориям
        var cpu = components.FirstOrDefault(c => c.Category == ComponentCategory.Processor);
        var motherboard = components.FirstOrDefault(c => c.Category == ComponentCategory.Motherboard);
        var ram = components.FirstOrDefault(c => c.Category == ComponentCategory.Ram);
        var gpu = components.FirstOrDefault(c => c.Category == ComponentCategory.Videocard);

        // проверка сокета 
        if (cpu != null && motherboard != null)
        {
            if (!string.IsNullOrEmpty(cpu.Socket) && !string.IsNullOrEmpty(motherboard.Socket))
            {
                if (cpu.Socket.ToLower() != motherboard.Socket.ToLower())
                {
                    errors.Add($"Несовместимость сокетов: Процессор ({cpu.Socket}) не подходит к Материнской плате ({motherboard.Socket}).");
                }
            }
        }

        //проверка памяти  =  проверка по строке DDR4/DDR5
        if (ram != null && motherboard != null)
        {
            var ramSpecs = ram.Specifications.ToLower();
            var moboSpecs = motherboard.Specifications.ToLower();

            bool isDdr4Ram = ramSpecs.Contains("ddr4");
            bool isDdr5Ram = ramSpecs.Contains("ddr5");

            //упоминание типа памяти в материнке
            bool moboSupportsDdr4 = moboSpecs.Contains("ddr4");
            bool moboSupportsDdr5 = moboSpecs.Contains("ddr5");

            if (isDdr4Ram && !moboSupportsDdr4)
                errors.Add($"Несовместимость памяти: ОЗУ DDR4 не подходит к этой материнской плате.");

            if (isDdr5Ram && !moboSupportsDdr5)
                errors.Add($"Несовместимость памяти: ОЗУ DDR5 не подходит к этой материнской плате.");
        }

        //подсчет энергопотребления 
        int totalPower = 0;
        if (cpu != null) totalPower += cpu.PowerConsumption;
        if (gpu != null) totalPower += gpu.PowerConsumption;

        if (totalPower > 0)
        {
            warnings.Add($"Рекомендуемый блок питания: от {totalPower + 100} Вт (с запасом).");
        }

        bool isCompatible = !errors.Any();

        return Ok(new
        {
            isCompatible,
            message = isCompatible ? "Конфигурация совместима!" : "Обнаружены критические ошибки совместимости.",
            errors,
            warnings,
            totalPowerConsumption = totalPower,
            components = components.Select(c => new { c.Id, c.Name, c.Category })
        });
    }

    // POST: api/components (Только для админов)
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateComponent([FromBody] Component newComponent)
    {
        if (newComponent == null)
            return BadRequest(new { message = "Данные компонента не предоставлены" });

        _db.Components.Add(newComponent);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetComponentById), new { id = newComponent.Id }, newComponent);
    }

    // PUT: api/components/{id} (Только для админов)
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateComponent(string id, [FromBody] Component updatedComponent)
    {
        if (updatedComponent == null)
            return BadRequest(new { message = "Данные компонента не предоставлены" });

        var component = await _db.Components.FindAsync(id);
        if (component == null)
            return NotFound(new { message = "Компонент не найден" });

        component.Name = updatedComponent.Name;
        component.Category = updatedComponent.Category;
        component.Price = updatedComponent.Price;
        component.Currency = updatedComponent.Currency;
        component.Specifications = updatedComponent.Specifications;
        component.Socket = updatedComponent.Socket;
        component.PowerConsumption = updatedComponent.PowerConsumption;
        component.ImageUrl = updatedComponent.ImageUrl;

        await _db.SaveChangesAsync();

        return Ok(component);
    }

    // DELETE: api/components/{id} (Только для админов)
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteComponent(string id)
    {
        var component = await _db.Components.FindAsync(id);
        if (component == null)
            return NotFound(new { message = "Компонент не найден" });

        _db.Components.Remove(component);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Компонент удалён" });
    }
}

// модель для запроса совместимости
public class BuildRequest
{
    public List<string> ComponentIds { get; set; } = new List<string>();
}