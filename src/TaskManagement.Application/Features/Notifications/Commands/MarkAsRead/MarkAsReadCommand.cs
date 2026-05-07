using MediatR;

namespace TaskManagement.Application.Features.Notifications.Commands.MarkAsRead;

public sealed record MarkAsReadCommand(Guid NotificationId) : IRequest<Unit>;
