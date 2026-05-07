using MediatR;

namespace TaskManagement.Application.Features.Notifications.Commands.MarkAllAsRead;

public sealed record MarkAllAsReadCommand : IRequest<int>;
