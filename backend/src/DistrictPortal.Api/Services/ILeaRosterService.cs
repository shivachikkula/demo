using DistrictPortal.Api.Contracts;

namespace DistrictPortal.Api.Services;

public interface ILeaRosterService
{
    Task<AdminRosterResponseDto> GetRosterAsync(string? search, string? sortBy, string? sortDir, CancellationToken cancellationToken);
}
