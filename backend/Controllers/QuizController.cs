using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcComponentsApi.Data;
using PcComponentsApi.Models;
namespace PcComponentsApi.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
[ApiController]
[Route("api/[controller]")]

public class QuizController : ControllerBase{
    private readonly AppDbContext _db;
    public QuizController(AppDbContext db)
    {
        _db = db;
    }
//Получение вопросов
[HttpGet]
public async Task<IActionResult> GetQuestions()
{
    var questions = await _db.QuizQuestions.ToListAsync();
    return Ok(questions);
}

//Отправка ответов
[Authorize]
[HttpPost("submit")]
public async Task<IActionResult> Submit([FromBody] Dictionary<string, int> answers)
{
    var questions = await _db.QuizQuestions.ToListAsync();
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    int score = 0;

    foreach (var q in questions)
    {
        if (answers.ContainsKey(q.Id) && answers[q.Id] == q.CorrectOptionIndex)
            score++;
    }

    var result = new QuizResult
    {
        UserId = userId,
        Score = score,
        TotalQuestions = questions.Count,
        CompletedAt = DateTime.UtcNow
    };

    _db.QuizResults.Add(result);
    await _db.SaveChangesAsync();

    return Ok(result);
}

//История результатов
[Authorize]
[HttpGet("results")]
public async Task<IActionResult> GetResults()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    var results = await _db.QuizResults
        .Where(r => r.UserId == userId)
        .ToListAsync();

    return Ok(results);
}
}