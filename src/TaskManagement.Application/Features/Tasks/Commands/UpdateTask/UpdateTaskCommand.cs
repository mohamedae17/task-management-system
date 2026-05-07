using MediatR;
using TaskManagement.Application.Features.Tasks.Dtos;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Tasks.Commands.UpdateTask;

public sealed record UpdateTaskCommand(
    Guid TaskId,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    Guid? AssigneeId,
    IReadOnlyList<Guid>? LabelIds) : IRequest<TaskDto>;
