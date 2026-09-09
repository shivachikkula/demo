namespace DistrictPortal.Api.Contracts;

public sealed record LeaRosterEntryDto
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string StatusColor { get; init; }

    public required int OverdueCount { get; init; }

    public required int FailedCount { get; init; }

    public required int ProcessingCount { get; init; }

    public required int ActiveCount { get; init; }

    public required int InactiveCount { get; init; }

    public required IReadOnlyList<string> MatchedCollectionNames { get; init; }
}

public sealed record LeaRosterSummaryDto
{
    public required int TotalLeas { get; init; }

    public required int OverdueLeas { get; init; }

    public required int FailedLeas { get; init; }

    public required int CompliantLeas { get; init; }
}

public sealed record AdminRosterResponseDto
{
    public required LeaRosterSummaryDto Summary { get; init; }

    public required IReadOnlyList<LeaRosterEntryDto> Leas { get; init; }
}
