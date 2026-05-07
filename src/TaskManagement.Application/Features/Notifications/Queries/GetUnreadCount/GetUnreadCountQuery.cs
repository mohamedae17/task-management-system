using MediatR;

namespace TaskManagement.Application.Features.Notifications.Queries.GetUnreadCount;

public sealed record GetUnreadCountQuery : IRequest<int>;
