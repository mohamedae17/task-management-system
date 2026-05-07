using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Users.Commands.SetUserActive;
using TaskManagement.Application.Features.Users.Commands.SetUserRoles;
using TaskManagement.Application.Features.Users.Commands.UpdateProfile;
using TaskManagement.Application.Features.Users.Dtos;
using TaskManagement.Application.Features.Users.Queries.GetAssignableUsers;
using TaskManagement.Application.Features.Users.Queries.GetCurrentUser;
using TaskManagement.Application.Features.Users.Queries.GetUserById;
using TaskManagement.Application.Features.Users.Queries.GetUsers;
using TaskManagement.Domain.Constants;

namespace TaskManagement.Api.Controllers;

[Route("api/users")]
[Authorize]
public sealed class UsersController : ApiControllerBase
{
    public sealed record UpdateProfileRequest(string FirstName, string LastName, string? AvatarUrl);
    public sealed record SetRolesRequest(IReadOnlyList<string> Roles);
    public sealed record SetActiveRequest(bool IsActive);

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var user = await Mediator.Send(new GetCurrentUserQuery(), ct);
        return Ok(user);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateProfileRequest body, CancellationToken ct)
    {
        var user = await Mediator.Send(
            new UpdateProfileCommand(body.FirstName, body.LastName, body.AvatarUrl), ct);
        return Ok(user);
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<ActionResult<PaginatedList<UserDto>>> List([FromQuery] GetUsersQuery query, CancellationToken ct)
    {
        return Ok(await Mediator.Send(query, ct));
    }

    [HttpGet("assignable")]
    public async Task<ActionResult<IReadOnlyList<UserSummaryDto>>> Assignable([FromQuery] string? search, CancellationToken ct)
    {
        return Ok(await Mediator.Send(new GetAssignableUsersQuery(search), ct));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<ActionResult<UserDto>> Get(Guid id, CancellationToken ct)
    {
        return Ok(await Mediator.Send(new GetUserByIdQuery(id), ct));
    }

    [HttpPut("{id:guid}/roles")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> SetRoles(Guid id, [FromBody] SetRolesRequest body, CancellationToken ct)
    {
        await Mediator.Send(new SetUserRolesCommand(id, body.Roles), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/active")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] SetActiveRequest body, CancellationToken ct)
    {
        await Mediator.Send(new SetUserActiveCommand(id, body.IsActive), ct);
        return NoContent();
    }
}
