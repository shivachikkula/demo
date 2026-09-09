namespace DistrictPortal.Api.Data.Entities;

/// <summary>A bell-menu notification. Null <see cref="LeaId"/> means it applies district-wide.</summary>
public sealed class NotificationEntity
{
    public required Guid Id { get; init; }

    public required NotificationSeverity Severity { get; set; }

    public required string Title { get; set; }

    public required string Message { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }

    public required bool IsRead { get; set; }

    public string? LeaId { get; set; }

    public string? CollectionId { get; set; }

    public Guid? SubmissionId { get; set; }
}
