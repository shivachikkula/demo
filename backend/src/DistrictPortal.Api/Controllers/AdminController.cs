using DistrictPortal.Api.Contracts;
using DistrictPortal.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistrictPortal.Api.Controllers;

/// <summary>Serves the Admin "All LEAs" roster view.</summary>
[ApiController]
[Route("api/admin")]
public sealed class AdminController(ILeaRosterService rosterService) : ControllerBase
{
    [HttpGet("leas")]
    public async Task<ActionResult<AdminRosterResponseDto>> GetLeaRoster(
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        CancellationToken cancellationToken)
    {
        return Ok(await rosterService.GetRosterAsync(search, sortBy, sortDir, cancellationToken));
    }
}
