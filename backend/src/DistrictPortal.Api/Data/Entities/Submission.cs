namespace DistrictPortal.Api.Data.Entities;

/// <summary>
/// One uploaded file for a collection: where it lives in blob storage, its validation
/// status, and the counts produced once processing finishes.
/// </summary>
public sealed class Submission
{
    public required Guid Id { get; init; }

    public required string CollectionId { get; set; }

    public CollectionDefinition? Collection { get; init; }

    public required string LeaId { get; set; }

    public required string FileName { get; set; }

    public required string BlobPath { get; set; }

    public required long SizeBytes { get; set; }

    public required string UploadedBy { get; set; }

    public required DateTimeOffset UploadedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public required SubmissionStatus Status { get; set; }

    public int? TotalRecords { get; set; }

    public int? PassedRecords { get; set; }

    public int? FailedRecords { get; set; }

    public int? WarningRecords { get; set; }
}
