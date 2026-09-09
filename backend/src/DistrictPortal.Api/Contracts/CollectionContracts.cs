namespace DistrictPortal.Api.Contracts;

public sealed record CollectionSummaryDto
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Status { get; init; }

    public required string StatusLabel { get; init; }

    public required string DueDate { get; init; }

    public int? RecordCount { get; init; }
}

public sealed record StatTileDto
{
    public required string Label { get; init; }

    public required int Value { get; init; }

    public required string Tone { get; init; }
}

public sealed record SubmissionDto
{
    public required Guid Id { get; init; }

    public required string FileName { get; init; }

    public required string UploadedAt { get; init; }

    public required string UploadedBy { get; init; }

    public required int? RowCount { get; init; }

    public required long SizeBytes { get; init; }

    public required string Status { get; init; }

    public required bool Passed { get; init; }
}

public sealed record CollectionDetailDto
{
    public required string OrgLabel { get; init; }

    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string SchoolYear { get; init; }

    public required string OverallStatus { get; init; }

    public required IReadOnlyList<StatTileDto> Stats { get; init; }

    public required SubmissionDto? LastUploadedFile { get; init; }

    public required IReadOnlyList<SubmissionDto> SubmissionHistory { get; init; }
}

public sealed record LeaDashboardDto
{
    public required string Id { get; init; }

    public required string OrgLabel { get; init; }

    public required string DisplayName { get; init; }

    public required IReadOnlyList<CollectionSummaryDto> ActiveCollections { get; init; }

    public required IReadOnlyList<CollectionSummaryDto> InactiveCollections { get; init; }
}
