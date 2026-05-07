using MediatR;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Tasks.Commands.ChangeTaskStatus;

public sealed record ChangeTaskStatusCommand(Guid TaskId, TaskItemStatus Status) : IRequest<Unit>;
