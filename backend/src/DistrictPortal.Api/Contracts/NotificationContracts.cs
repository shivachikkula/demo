namespace DistrictPortal.Api.Contracts;

public sealed record NotificationDto
{
    public required Guid Id { get; init; }

    public required string Severity { get; init; }

    public required string Title { get; init; }

    public required string Message { get; init; }

    public required string Timestamp { get; init; }

    public required bool Read { get; init; }

    public string? LeaId { get; init; }

    public string? CollectionId { get; init; }
}

/// <summary>Pushed over SignalR to the LEA's group when a submission finishes processing.</summary>
public sealed record SubmissionStatusChangedMessage
{
    public required string LeaId { get; init; }

    public required string CollectionId { get; init; }

    public required SubmissionDto Submission { get; init; }

    public required CollectionDetailDto CollectionDetail { get; init; }
}
