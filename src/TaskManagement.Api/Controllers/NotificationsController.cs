using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Notifications.Commands.MarkAllAsRead;
using TaskManagement.Application.Features.Notifications.Commands.MarkAsRead;
using TaskManagement.Application.Features.Notifications.Dtos;
using TaskManagement.Application.Features.Notifications.Queries.GetMyNotifications;
using TaskManagement.Application.Features.Notifications.Queries.GetUnreadCount;

namespace TaskManagement.Api.Controllers;

[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedList<NotificationDto>>> List([FromQuery] GetMyNotificationsQuery query, CancellationToken ct) =>
        Ok(await Mediator.Send(query, ct));

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> UnreadCount(CancellationToken ct) =>
        Ok(await Mediator.Send(new GetUnreadCountQuery(), ct));

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new MarkAsReadCommand(id), ct);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<int>> MarkAllRead(CancellationToken ct) =>
        Ok(new { updated = await Mediator.Send(new MarkAllAsReadCommand(), ct) });
}
