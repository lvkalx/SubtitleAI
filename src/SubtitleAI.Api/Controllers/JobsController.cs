using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubtitleAI.Application.Jobs;
using SubtitleAI.Domain;

namespace SubtitleAI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/jobs")]
public class JobsController(IJobService jobs) : ControllerBase
{
    private const long MaxBytes = 500L * 1024 * 1024;

    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".m4a", ".ogg", ".flac", ".aac",
        ".mp4", ".mkv", ".mov", ".webm", ".avi"
    };

    private string UserId => User.FindFirstValue("sub")!;

    [HttpPost]
    [RequestSizeLimit(MaxBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxBytes)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { errors = new[] { "File is empty." } });

        var ext = Path.GetExtension(file.FileName);
        if (!Allowed.Contains(ext))
            return BadRequest(new { errors = new[] { $"Unsupported file type '{ext}'." } });

        await using var stream = file.OpenReadStream();
        var job = await jobs.CreateAsync(
            UserId, Path.GetFileName(file.FileName), file.ContentType, stream, ct);

        return AcceptedAtAction(nameof(Get), new { id = job.Id }, job);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            return BadRequest(new { errors = new[] { "page must be >= 1 and pageSize must be between 1 and 100." } });

        return Ok(await jobs.ListAsync(UserId, page, pageSize, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var job = await jobs.GetAsync(UserId, id, ct);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpGet("{id:guid}/transcript")]
    public async Task<IActionResult> GetTranscript(Guid id, CancellationToken ct)
    {
        var t = await jobs.GetTranscriptAsync(UserId, id, ct);
        return t is null ? NotFound() : Ok(new { text = t.FullText });
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<IActionResult> GetSummary(Guid id, CancellationToken ct)
    {
        var s = await jobs.GetSummaryAsync(UserId, id, ct);
        if (s is not null) return Ok(new SummaryDto(s.Text, s.Model, s.CreatedAt));

        var job = await jobs.GetAsync(UserId, id, ct);
        if (job is null) return NotFound();

        // A summary is created with the Done status, so an unfinished job returns 409.
        if (job.Status != nameof(JobStatus.Done))
            return Conflict(new { errors = new[] { $"Job is {job.Status}; summary is not available." } });

        // Subtitles are ready, but no summary exists because the LLM is unavailable or failed.
        return NoContent();
    }

    [HttpGet("{id:guid}/srt")]
    public async Task<IActionResult> Srt(Guid id, CancellationToken ct)
    {
        var t = await jobs.GetTranscriptAsync(UserId, id, ct);
        return t is null
            ? NotFound()
            : File(Encoding.UTF8.GetBytes(t.SrtContent), "application/x-subrip", $"{id}.srt");
    }

    [HttpGet("{id:guid}/vtt")]
    public async Task<IActionResult> Vtt(Guid id, CancellationToken ct)
    {
        var t = await jobs.GetTranscriptAsync(UserId, id, ct);
        return t is null
            ? NotFound()
            : File(Encoding.UTF8.GetBytes(t.VttContent), "text/vtt", $"{id}.vtt");
    }
}
