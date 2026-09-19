using DistrictPortal.Api.Contracts;
using DistrictPortal.Api.Data;
using DistrictPortal.Api.Data.Entities;
using DistrictPortal.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DistrictPortal.Api.Controllers;

/// <summary>
/// Serves one LEA's dashboard: its collections, a collection's detail/history, and file
/// uploads. Used by both the Sponsor view (always called with the caller's own LEA id)
/// and the Admin view's drill-in page (called with whichever LEA id the admin selected).
/// </summary>
[ApiController]
[Route("api/leas")]
public sealed class LeaDashboardController(
    AppDbContext db,
    IBlobStorageService blobStorage,
    ISubmissionProcessingQueue processingQueue,
    TimeProvider timeProvider) : ControllerBase
{
    private const string DefaultUploadedBy = "shiva.chikkula@dc.gov";
    private const long MaxUploadBytes = 50 * 1024 * 1024;

    [HttpGet("{leaId}")]
    public async Task<ActionResult<LeaDashboardDto>> GetDashboard(string leaId, CancellationToken cancellationToken)
    {
        var lea = await db.Leas
            .Include(l => l.Collections)
            .ThenInclude(c => c.Submissions)
            .FirstOrDefaultAsync(l => l.Id == leaId, cancellationToken);

        return lea is null ? NotFound() : Ok(lea.ToDashboardDto());
    }

    [HttpGet("{leaId}/collections/{collectionId}")]
    public async Task<ActionResult<CollectionDetailDto>> GetCollectionDetail(
        string leaId,
        string collectionId,
        CancellationToken cancellationToken)
    {
        var collection = await FindCollectionAsync(leaId, collectionId, cancellationToken);
        return collection is null ? NotFound() : Ok(collection.ToDetailDto());
    }

    [HttpGet("{leaId}/collections/{collectionId}/submissions")]
    public async Task<ActionResult<IReadOnlyList<SubmissionDto>>> GetSubmissionHistory(
        string leaId,
        string collectionId,
        CancellationToken cancellationToken)
    {
        var exists = await db.Collections.AnyAsync(c => c.Id == collectionId && c.LeaId == leaId, cancellationToken);
        if (!exists)
        {
            return NotFound();
        }

        // SQLite can't translate ORDER BY on DateTimeOffset server-side, so order client-side.
        var submissions = await db.Submissions
            .Where(s => s.CollectionId == collectionId && s.LeaId == leaId)
            .ToListAsync(cancellationToken);

        return Ok(submissions.OrderByDescending(s => s.UploadedAt).Select(s => s.ToSubmissionDto()).ToList());
    }

    /// <summary>
    /// Stores the file in blob storage, records it as "Processing", and hands it off to the
    /// background worker. The caller gets an immediate 202 - the eventual pass/fail result
    /// arrives over SignalR instead of the client having to poll for it.
    /// </summary>
    [HttpPost("{leaId}/collections/{collectionId}/submissions")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<ActionResult<SubmissionDto>> UploadSubmission(
        string leaId,
        string collectionId,
        [FromForm] IFormFile file,
        [FromForm] string? uploadedBy,
        CancellationToken cancellationToken)
    {
        var collection = await db.Collections.FirstOrDefaultAsync(
            c => c.Id == collectionId && c.LeaId == leaId, cancellationToken);

        if (collection is null)
        {
            return NotFound();
        }

        if (file.Length == 0)
        {
            return BadRequest("The uploaded file is empty.");
        }

        var submissionId = Guid.CreateVersion7();
        var blobPath = $"submissions/{leaId}/{collectionId}/{submissionId}-{file.FileName}";

        await using (var stream = file.OpenReadStream())
        {
            await blobStorage.UploadAsync(blobPath, stream, file.ContentType, cancellationToken);
        }

        var submission = new Submission
        {
            Id = submissionId,
            CollectionId = collectionId,
            LeaId = leaId,
            FileName = file.FileName,
            BlobPath = blobPath,
            SizeBytes = file.Length,
            UploadedBy = string.IsNullOrWhiteSpace(uploadedBy) ? DefaultUploadedBy : uploadedBy,
            UploadedAt = timeProvider.GetUtcNow(),
            Status = SubmissionStatus.Processing,
        };

        db.Submissions.Add(submission);
        await db.SaveChangesAsync(cancellationToken);

        processingQueue.Enqueue(submission.Id);

        return AcceptedAtAction(
            nameof(GetSubmissionHistory),
            new { leaId, collectionId },
            submission.ToSubmissionDto());
    }

    [HttpGet("{leaId}/collections/{collectionId}/submissions/{submissionId:guid}/download")]
    public async Task<IActionResult> DownloadSubmission(
        string leaId,
        string collectionId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var submission = await db.Submissions.FirstOrDefaultAsync(
            s => s.Id == submissionId && s.CollectionId == collectionId && s.LeaId == leaId,
            cancellationToken);

        if (submission is null)
        {
            return NotFound();
        }

        var stream = await blobStorage.DownloadAsync(submission.BlobPath, cancellationToken);
        return File(stream, "application/octet-stream", submission.FileName);
    }

    private Task<CollectionDefinition?> FindCollectionAsync(string leaId, string collectionId, CancellationToken cancellationToken) =>
        db.Collections
            .Include(c => c.Lea)
            .Include(c => c.Submissions)
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.LeaId == leaId, cancellationToken);
}
