namespace DistrictPortal.Api.Data.Entities;

/// <summary>A data collection (e.g. Discipline, Course) that an LEA submits files against.</summary>
public sealed class CollectionDefinition
{
    public required string Id { get; init; }

    public required string LeaId { get; set; }

    public Lea? Lea { get; init; }

    public required string Name { get; set; }

    public required DateOnly DueDate { get; set; }

    public required bool IsActive { get; set; }

    public required string SchoolYear { get; set; }

    /// <summary>Display order within its LEA (lower first). Ties broken by name.</summary>
    public required int SortOrder { get; set; }

    /// <summary>
    /// Status/label to show when this collection has no submissions yet (e.g. "overdue" /
    /// "Overdue" for a missed due date, or "not-started" / "Not started"). Ignored once a
    /// submission exists - status is then derived from the latest submission.
    /// </summary>
    public string? NoSubmissionStatus { get; set; }

    public string? NoSubmissionStatusLabel { get; set; }

    public List<Submission> Submissions { get; init; } = [];
}
