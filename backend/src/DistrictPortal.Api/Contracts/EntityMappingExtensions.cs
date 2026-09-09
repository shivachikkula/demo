using System.Globalization;
using DistrictPortal.Api.Data.Entities;

namespace DistrictPortal.Api.Contracts;

/// <summary>Maps EF Core entities onto the DTOs the API returns.</summary>
public static class EntityMappingExtensions
{
    private const string DateFormat = "MMM d, yyyy";
    private const string DateTimeFormat = "MMM d, yyyy, h:mm tt";

    public static LeaDashboardDto ToDashboardDto(this Lea lea) => new()
    {
        Id = lea.Id,
        OrgLabel = lea.OrgLabel,
        DisplayName = lea.DisplayName,
        ActiveCollections = lea.Collections.Where(c => c.IsActive).Select(c => c.ToSummaryDto()).ToList(),
        InactiveCollections = lea.Collections.Where(c => !c.IsActive).Select(c => c.ToSummaryDto()).ToList(),
    };

    public static CollectionSummaryDto ToSummaryDto(this CollectionDefinition collection)
    {
        var latest = collection.LatestSubmission();
        var (status, statusLabel) = collection.ResolveStatus(latest);

        return new CollectionSummaryDto
        {
            Id = collection.Id,
            Name = collection.Name,
            Status = status,
            StatusLabel = statusLabel,
            DueDate = collection.DueDate.ToDateTime(TimeOnly.MinValue).ToString(DateFormat, CultureInfo.InvariantCulture),
            RecordCount = latest?.TotalRecords,
        };
    }

    public static CollectionDetailDto ToDetailDto(this CollectionDefinition collection)
    {
        var submissions = collection.Submissions.OrderByDescending(s => s.UploadedAt).ToList();
        var latest = submissions.FirstOrDefault();

        var overallStatus = latest switch
        {
            null => "Failed",
            { Status: SubmissionStatus.Passed } => "Passed",
            { Status: SubmissionStatus.Failed } => "Failed",
            _ => "Processing",
        };

        return new CollectionDetailDto
        {
            OrgLabel = collection.Lea?.OrgLabel ?? collection.LeaId,
            Id = collection.Id,
            Name = $"{collection.Name} Collection",
            SchoolYear = collection.SchoolYear,
            OverallStatus = overallStatus,
            Stats =
            [
                new StatTileDto { Label = "Total records", Value = latest?.TotalRecords ?? 0, Tone = "neutral" },
                new StatTileDto { Label = "Passed", Value = latest?.PassedRecords ?? 0, Tone = "positive" },
                new StatTileDto { Label = "Failed", Value = latest?.FailedRecords ?? 0, Tone = "negative" },
                new StatTileDto { Label = "Warnings", Value = latest?.WarningRecords ?? 0, Tone = "warning" },
            ],
            LastUploadedFile = latest?.ToSubmissionDto(),
            SubmissionHistory = submissions.Select(s => s.ToSubmissionDto()).ToList(),
        };
    }

    public static SubmissionDto ToSubmissionDto(this Submission submission) => new()
    {
        Id = submission.Id,
        FileName = submission.FileName,
        UploadedAt = submission.UploadedAt.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
        UploadedBy = submission.UploadedBy,
        RowCount = submission.TotalRecords,
        SizeBytes = submission.SizeBytes,
        Status = submission.Status.ToString(),
        Passed = submission.Status == SubmissionStatus.Passed,
    };

    public static NotificationDto ToDto(this NotificationEntity notification) => new()
    {
        Id = notification.Id,
        Severity = notification.Severity.ToString().ToLowerInvariant(),
        Title = notification.Title,
        Message = notification.Message,
        Timestamp = notification.CreatedAt.ToString(DateFormat, CultureInfo.InvariantCulture),
        Read = notification.IsRead,
        LeaId = notification.LeaId,
        CollectionId = notification.CollectionId,
    };

    private static Submission? LatestSubmission(this CollectionDefinition collection) =>
        collection.Submissions.OrderByDescending(s => s.UploadedAt).FirstOrDefault();

    private static (string Status, string StatusLabel) ResolveStatus(this CollectionDefinition collection, Submission? latest)
    {
        if (!collection.IsActive)
        {
            return ("success", "Closed");
        }

        if (latest is null)
        {
            return (collection.NoSubmissionStatus ?? "not-started", collection.NoSubmissionStatusLabel ?? "Not started");
        }

        return latest.Status switch
        {
            SubmissionStatus.Passed => ("success", "Success"),
            SubmissionStatus.Failed => ("failed", "Failed"),
            _ => ("processing", "Processing"),
        };
    }
}
