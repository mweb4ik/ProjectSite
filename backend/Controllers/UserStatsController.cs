using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcComponentsApi.Data;
using PcComponentsApi.Models;
using System.Security.Claims;

[ApiController]
[Route("api/user-stats")]
public class UserStatsController : ControllerBase
{
    private readonly AppDbContext _db;

    public UserStatsController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize]
    [HttpPost("track-component")]
    public async Task<IActionResult> TrackComponent([FromBody] string componentId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        bool alreadyViewed = await _db.ComponentViews
            .AnyAsync(v => v.UserId == userId && v.ComponentId == componentId);

        if (!alreadyViewed)
        {
            _db.ComponentViews.Add(new ComponentView
            {
                UserId = userId,
                ComponentId = componentId
            });

            await _db.SaveChangesAsync();
        }

        return Ok();
    }

    [Authorize]
    [HttpGet("profile-stats")]
    public async Task<IActionResult> GetProfileStats()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var viewed = await _db.ComponentViews
            .Where(v => v.UserId == userId)
            .Select(v => v.ComponentId)
            .Distinct()
            .CountAsync();

        var totalComponents = await _db.Components.CountAsync();

        var quizResults = await _db.QuizResults
            .Where(q => q.UserId == userId)
            .ToListAsync();

        return Ok(new
        {
            viewedComponents = viewed,
            totalComponents,
            progress = totalComponents == 0 ? 0 : (viewed * 100 / totalComponents),

            totalQuizAttempts = quizResults.Count,

            bestQuizScore = quizResults.Any()
                ? quizResults.Max(q => q.Score)
                : 0,

            averageQuizScore = quizResults.Any()
                ? Math.Round(quizResults.Average(q => q.Score), 1)
                : 0,

            recentQuizResults = quizResults
                .OrderByDescending(q => q.CompletedAt)
                .Take(5)
        });
    }
}