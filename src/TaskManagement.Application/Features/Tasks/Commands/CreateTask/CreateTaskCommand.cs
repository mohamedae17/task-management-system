using MediatR;
using TaskManagement.Application.Features.Tasks.Dtos;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Tasks.Commands.CreateTask;

public sealed record CreateTaskCommand(
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    Guid? AssigneeId,
    Guid? ParentTaskId,
    IReadOnlyList<Guid>? LabelIds) : IRequest<TaskDto>;
