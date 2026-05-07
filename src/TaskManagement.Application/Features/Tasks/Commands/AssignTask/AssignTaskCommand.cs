using MediatR;

namespace TaskManagement.Application.Features.Tasks.Commands.AssignTask;

public sealed record AssignTaskCommand(Guid TaskId, Guid? AssigneeId) : IRequest<Unit>;
