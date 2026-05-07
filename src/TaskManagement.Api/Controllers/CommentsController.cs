using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Comments.Commands.CreateComment;
using TaskManagement.Application.Features.Comments.Commands.DeleteComment;
using TaskManagement.Application.Features.Comments.Commands.UpdateComment;
using TaskManagement.Application.Features.Comments.Dtos;
using TaskManagement.Application.Features.Comments.Queries.GetTaskComments;

namespace TaskManagement.Api.Controllers;

[Route("api")]
[Authorize]
public sealed class CommentsController : ApiControllerBase
{
    public sealed record CreateCommentRequest(string Content);
    public sealed record UpdateCommentRequest(string Content);

    [HttpGet("tasks/{taskId:guid}/comments")]
    public async Task<ActionResult<PaginatedList<CommentDto>>> List(
        Guid taskId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var query = new GetTaskCommentsQuery(taskId) { PageNumber = pageNumber, PageSize = pageSize };
        return Ok(await Mediator.Send(query, ct));
    }

    [HttpPost("tasks/{taskId:guid}/comments")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CommentDto>> Create(
        Guid taskId, [FromBody] CreateCommentRequest body, CancellationToken ct)
    {
        var dto = await Mediator.Send(new CreateCommentCommand(taskId, body.Content), ct);
        return Created($"/api/comments/{dto.Id}", dto);
    }

    [HttpPut("comments/{id:guid}")]
    public async Task<ActionResult<CommentDto>> Update(Guid id, [FromBody] UpdateCommentRequest body, CancellationToken ct) =>
        Ok(await Mediator.Send(new UpdateCommentCommand(id, body.Content), ct));

    [HttpDelete("comments/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteCommentCommand(id), ct);
        return NoContent();
    }
}
