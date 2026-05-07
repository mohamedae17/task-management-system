using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Features.Labels.Commands.CreateLabel;
using TaskManagement.Application.Features.Labels.Commands.DeleteLabel;
using TaskManagement.Application.Features.Labels.Commands.UpdateLabel;
using TaskManagement.Application.Features.Labels.Dtos;
using TaskManagement.Application.Features.Labels.Queries.GetLabels;
using TaskManagement.Domain.Constants;

namespace TaskManagement.Api.Controllers;

[Route("api/labels")]
[Authorize]
public sealed class LabelsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LabelDto>>> List(CancellationToken ct) =>
        Ok(await Mediator.Send(new GetLabelsQuery(), ct));

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<ActionResult<LabelDto>> Create([FromBody] CreateLabelCommand command, CancellationToken ct)
    {
        var dto = await Mediator.Send(command, ct);
        return Created($"/api/labels/{dto.Id}", dto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<ActionResult<LabelDto>> Update(Guid id, [FromBody] UpdateLabelCommand command, CancellationToken ct)
    {
        if (id != command.LabelId) return BadRequest("Route id and body LabelId must match.");
        return Ok(await Mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteLabelCommand(id), ct);
        return NoContent();
    }
}
