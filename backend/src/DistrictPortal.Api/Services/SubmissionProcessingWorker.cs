using DistrictPortal.Api.Contracts;
using DistrictPortal.Api.Data;
using DistrictPortal.Api.Data.Entities;
using DistrictPortal.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DistrictPortal.Api.Services;

/// <summary>
/// Simulates file validation for newly uploaded submissions, then persists the result and
/// pushes it to connected clients over SignalR - the replacement for 5-minute status polling.
/// </summary>
public sealed class SubmissionProcessingWorker(
    ISubmissionProcessingQueue queue,
    IServiceScopeFactory scopeFactory,
    IHubContext<FileProcessingHub> hub,
    TimeProvider timeProvider,
    ILogger<SubmissionProcessingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var submissionId in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessAsync(submissionId, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to process submission {SubmissionId}", submissionId);
            }
        }
    }

    private async Task ProcessAsync(Guid submissionId, CancellationToken stoppingToken)
    {
        // Simulate the time a real validation pipeline would take.
        var delay = TimeSpan.FromSeconds(Random.Shared.Next(3, 8));
        await Task.Delay(delay, timeProvider, stoppingToken);

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var submission = await db.Submissions
            .Include(s => s.Collection)
            .ThenInclude(c => c!.Lea)
            .FirstOrDefaultAsync(s => s.Id == submissionId, stoppingToken);

        if (submission is null)
        {
            logger.LogWarning("Submission {SubmissionId} disappeared before processing", submissionId);
            return;
        }

        ApplySimulatedResult(submission, timeProvider.GetUtcNow());
        await db.SaveChangesAsync(stoppingToken);

        var collection = await db.Collections
            .Include(c => c.Submissions)
            .Include(c => c.Lea)
            .FirstAsync(c => c.Id == submission.CollectionId, stoppingToken);

        var notification = BuildNotification(submission, collection);
        db.Notifications.Add(notification);
        await db.SaveChangesAsync(stoppingToken);

        var message = new SubmissionStatusChangedMessage
        {
            LeaId = submission.LeaId,
            CollectionId = submission.CollectionId,
            Submission = submission.ToSubmissionDto(),
            CollectionDetail = collection.ToDetailDto(),
        };

        await hub.Clients.Group(FileProcessingHub.LeaGroupName(submission.LeaId))
            .SendAsync(FileProcessingHub.SubmissionStatusChangedMethod, message, stoppingToken);

        await hub.Clients.Group(FileProcessingHub.GlobalGroupName())
            .SendAsync(FileProcessingHub.NotificationCreatedMethod, notification.ToDto(), stoppingToken);
    }

    private static void ApplySimulatedResult(Submission submission, DateTimeOffset processedAt)
    {
        var totalRecords = Math.Clamp((int)(submission.SizeBytes / 200), 50, 20_000);
        var isFailure = Random.Shared.NextDouble() < 0.2;

        var warningRecords = Random.Shared.Next(0, totalRecords / 25 + 1);
        var failedRecords = isFailure ? Random.Shared.Next(totalRecords / 10, Math.Max(totalRecords / 4, totalRecords / 10 + 1)) : 0;
        var passedRecords = totalRecords - failedRecords - warningRecords;

        submission.Status = isFailure ? SubmissionStatus.Failed : SubmissionStatus.Passed;
        submission.TotalRecords = totalRecords;
        submission.PassedRecords = passedRecords;
        submission.FailedRecords = failedRecords;
        submission.WarningRecords = warningRecords;
        submission.ProcessedAt = processedAt;
    }

    private static NotificationEntity BuildNotification(Submission submission, CollectionDefinition collection)
    {
        var passed = submission.Status == SubmissionStatus.Passed;

        return new NotificationEntity
        {
            Id = Guid.CreateVersion7(),
            Severity = passed ? NotificationSeverity.Info : NotificationSeverity.Error,
            Title = passed
                ? $"{collection.Name} collection passed validation"
                : $"{collection.Name} upload failed",
            Message = passed
                ? $"{submission.FileName} passed with {submission.PassedRecords:N0} of {submission.TotalRecords:N0} records clean."
                : $"{submission.FileName} had {submission.FailedRecords:N0} validation errors. Review and resubmit.",
            CreatedAt = submission.ProcessedAt ?? DateTimeOffset.UtcNow,
            IsRead = false,
            LeaId = submission.LeaId,
            CollectionId = submission.CollectionId,
            SubmissionId = submission.Id,
        };
    }
}
