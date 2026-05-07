using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Tasks.Commands.AssignTask;
using TaskManagement.Application.Features.Tasks.Commands.ChangeTaskStatus;
using TaskManagement.Application.Features.Tasks.Commands.CreateTask;
using TaskManagement.Application.Features.Tasks.Commands.DeleteTask;
using TaskManagement.Application.Features.Tasks.Commands.UpdateTask;
using TaskManagement.Application.Features.Tasks.Dtos;
using TaskManagement.Application.Features.Tasks.Queries.GetMyTasks;
using TaskManagement.Application.Features.Tasks.Queries.GetTaskById;
using TaskManagement.Application.Features.Tasks.Queries.GetTasks;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Api.Controllers;

[Route("api/tasks")]
[Authorize]
public sealed class TasksController : ApiControllerBase
{
    public sealed record AssignRequest(Guid? AssigneeId);
    public sealed record ChangeStatusRequest(TaskItemStatus Status);

    [HttpGet]
    public async Task<ActionResult<PaginatedList<TaskListItemDto>>> List([FromQuery] GetTasksQuery query, CancellationToken ct) =>
        Ok(await Mediator.Send(query, ct));

    [HttpGet("mine")]
    public async Task<ActionResult<PaginatedList<TaskListItemDto>>> Mine([FromQuery] GetMyTasksQuery query, CancellationToken ct) =>
        Ok(await Mediator.Send(query, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> Get(Guid id, CancellationToken ct) =>
        Ok(await Mediator.Send(new GetTaskByIdQuery(id), ct));

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskCommand command, CancellationToken ct)
    {
        var task = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskDto>> Update(Guid id, [FromBody] UpdateTaskCommand command, CancellationToken ct)
    {
        if (id != command.TaskId) return BadRequest("Route id and body TaskId must match.");
        var updated = await Mediator.Send(command, ct);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteTaskCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignRequest body, CancellationToken ct)
    {
        await Mediator.Send(new AssignTaskCommand(id, body.AssigneeId), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest body, CancellationToken ct)
    {
        await Mediator.Send(new ChangeTaskStatusCommand(id, body.Status), ct);
        return NoContent();
    }
}
