using MediatR;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Notifications.Dtos;

namespace TaskManagement.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed class GetMyNotificationsQuery : PaginationRequest, IRequest<PaginatedList<NotificationDto>>
{
    public bool? UnreadOnly { get; set; }

    public GetMyNotificationsQuery()
    {
        PageSize = 20;
    }
}
