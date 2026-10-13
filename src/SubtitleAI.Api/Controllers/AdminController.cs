using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubtitleAI.Domain;
using SubtitleAI.Infrastructure.Persistence;

namespace SubtitleAI.Api.Controllers;

[ApiController]
[Authorize(Policy = "admin")]
[Route("api/admin")]
public class AdminController(AppDbContext db, UserManager<ApplicationUser> users) : ControllerBase
{
    [HttpGet("analytics")]
    public async Task<IActionResult> Analytics(CancellationToken ct)
    {
        var since = DateTime.UtcNow.Date.AddDays(-6);
        var jobs = db.MediaJobs.AsNoTracking();
        var daily = await jobs
            .Where(job => job.CreatedAt >= since)
            .GroupBy(job => job.CreatedAt.Date)
            .Select(group => new { Date = group.Key, Count = group.Count() })
            .ToListAsync(ct);

        var dailyCounts = daily.ToDictionary(item => item.Date, item => item.Count);
        var activity = Enumerable.Range(0, 7)
            .Select(daysAgo => since.AddDays(daysAgo))
            .Select(date => new { date = date.ToString("yyyy-MM-dd"), count = dailyCounts.GetValueOrDefault(date) })
            .ToArray();

        return Ok(new
        {
            users = await users.Users.CountAsync(ct),
            usersLast30Days = await users.Users.CountAsync(user => user.CreatedAt >= DateTime.UtcNow.AddDays(-30), ct),
            transcriptions = await jobs.CountAsync(ct),
            completed = await jobs.CountAsync(job => job.Status == JobStatus.Done, ct),
            processing = await jobs.CountAsync(job => job.Status == JobStatus.Pending || job.Status == JobStatus.Processing, ct),
            failed = await jobs.CountAsync(job => job.Status == JobStatus.Failed, ct),
            transcripts = await db.Transcripts.CountAsync(ct),
            summaries = await db.Summaries.CountAsync(ct),
            totalMediaMinutes = Math.Round((await jobs.Where(job => job.DurationSeconds != null).SumAsync(job => job.DurationSeconds, ct) ?? 0) / 60, 1),
            activity
        });
    }
}
