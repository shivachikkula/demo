using DistrictPortal.Api.Contracts;
using DistrictPortal.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DistrictPortal.Api.Controllers;

/// <summary>Backs the header bell menu shared by both the Sponsor and Admin views.</summary>
[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetAll(CancellationToken cancellationToken)
    {
        // SQLite can't translate ORDER BY on DateTimeOffset server-side, so order client-side.
        var notifications = await db.Notifications.ToListAsync(cancellationToken);

        return Ok(notifications.OrderByDescending(n => n.CreatedAt).Select(n => n.ToDto()).ToList());
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var notification = await db.Notifications.FindAsync([id], cancellationToken);
        if (notification is null)
        {
            return NotFound();
        }

        notification.IsRead = true;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await db.Notifications
            .Where(n => !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true), cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Dismiss(Guid id, CancellationToken cancellationToken)
    {
        var affected = await db.Notifications.Where(n => n.Id == id).ExecuteDeleteAsync(cancellationToken);
        return affected == 0 ? NotFound() : NoContent();
    }
}
