using DistrictPortal.Api.Contracts;
using DistrictPortal.Api.Data;
using DistrictPortal.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DistrictPortal.Api.Services;

/// <summary>Builds the Admin "All LEAs" roster: aggregate stats plus a searchable, sortable per-LEA row.</summary>
public sealed class LeaRosterService(AppDbContext db) : ILeaRosterService
{
    // These four figures represent the full district (38 LEAs); only a handful have
    // full drill-in data seeded for this demo, so they're tracked independently of the
    // rows actually returned below.
    private static readonly LeaRosterSummaryDto DistrictWideSummary = new()
    {
        TotalLeas = 38,
        OverdueLeas = 6,
        FailedLeas = 4,
        CompliantLeas = 21,
    };

    public async Task<AdminRosterResponseDto> GetRosterAsync(
        string? search,
        string? sortBy,
        string? sortDir,
        CancellationToken cancellationToken)
    {
        var leas = await db.Leas
            .Include(lea => lea.Collections)
            .ThenInclude(collection => collection.Submissions)
            .ToListAsync(cancellationToken);

        var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToLowerInvariant();

        var rows = leas
            .Select(lea => BuildRow(lea, term))
            .Where(row => row.Matched)
            .Select(row => row.Dto)
            .ToList();

        return new AdminRosterResponseDto
        {
            Summary = DistrictWideSummary,
            Leas = Sort(rows, sortBy, sortDir),
        };
    }

    private static (LeaRosterEntryDto Dto, bool Matched) BuildRow(Lea lea, string? term)
    {
        var activeCollections = lea.Collections.Where(c => c.IsActive).ToList();
        var inactiveCount = lea.Collections.Count(c => !c.IsActive);

        var overdueCount = 0;
        var failedCount = 0;
        var processingCount = 0;

        foreach (var collection in activeCollections)
        {
            var latest = collection.Submissions.OrderByDescending(s => s.UploadedAt).FirstOrDefault();

            if (latest is null)
            {
                if (collection.NoSubmissionStatus == "overdue")
                {
                    overdueCount++;
                }
                continue;
            }

            switch (latest.Status)
            {
                case SubmissionStatus.Failed:
                    failedCount++;
                    break;
                case SubmissionStatus.Processing or SubmissionStatus.Uploaded:
                    processingCount++;
                    break;
            }
        }

        var statusColor = (overdueCount, failedCount) switch
        {
            (> 0, _) => "red",
            (_, > 0) => "amber",
            _ => "green",
        };

        var nameMatches = term is not null && lea.Name.ToLowerInvariant().Contains(term);
        var matchedCollectionNames = term is null
            ? []
            : lea.Collections
                .Select(c => c.Name)
                .Distinct()
                .Where(name => name.ToLowerInvariant().Contains(term))
                .ToList();

        var dto = new LeaRosterEntryDto
        {
            Id = lea.Id,
            Name = lea.Name,
            StatusColor = statusColor,
            OverdueCount = overdueCount,
            FailedCount = failedCount,
            ProcessingCount = processingCount,
            ActiveCount = activeCollections.Count,
            InactiveCount = inactiveCount,
            // Only surface the "matched via collection" note when the LEA's own name didn't match.
            MatchedCollectionNames = nameMatches ? [] : matchedCollectionNames,
        };

        var matched = term is null || nameMatches || matchedCollectionNames.Count > 0;
        return (dto, matched);
    }

    private static List<LeaRosterEntryDto> Sort(List<LeaRosterEntryDto> rows, string? sortBy, string? sortDir)
    {
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        IEnumerable<LeaRosterEntryDto> ordered = sortBy?.ToLowerInvariant() switch
        {
            "overduecount" => descending ? rows.OrderByDescending(r => r.OverdueCount) : rows.OrderBy(r => r.OverdueCount),
            "failedcount" => descending ? rows.OrderByDescending(r => r.FailedCount) : rows.OrderBy(r => r.FailedCount),
            "processingcount" => descending ? rows.OrderByDescending(r => r.ProcessingCount) : rows.OrderBy(r => r.ProcessingCount),
            _ => descending ? rows.OrderByDescending(r => r.Name) : rows.OrderBy(r => r.Name),
        };

        return ordered.ToList();
    }
}
