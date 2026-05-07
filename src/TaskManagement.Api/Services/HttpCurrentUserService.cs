using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Api.Services;

public sealed class HttpCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;
    private HttpContext? Context => _accessor.HttpContext;

    public Guid? UserId
    {
        get
        {
            var raw = Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                   ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public string? UserName =>
        Principal?.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
        ?? Principal?.Identity?.Name;

    public string? Email =>
        Principal?.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? Principal?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        ?? (IReadOnlyList<string>)Array.Empty<string>();

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;

    public string? IpAddress
    {
        get
        {
            var ctx = Context;
            if (ctx is null) return null;

            // Trust X-Forwarded-For only when populated by an upstream proxy you control.
            // For dev simplicity we accept it but cap at the first hop.
            if (ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var fwd) && fwd.Count > 0)
                return fwd[0]!.Split(',')[0].Trim();

            return ctx.Connection.RemoteIpAddress?.ToString();
        }
    }

    public string? UserAgent =>
        Context?.Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;
}
