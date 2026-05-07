using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected string? IpAddress() =>
        HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var fwd) && fwd.Count > 0
            ? fwd[0]!.Split(',')[0].Trim()
            : HttpContext.Connection.RemoteIpAddress?.ToString();
}
