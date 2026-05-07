using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Infrastructure.Services;

/// Used for design-time operations (e.g. EF migrations) and as a safe fallback
/// when no HTTP context is available. The real implementation lives in the API layer.
public sealed class AnonymousCurrentUserService : ICurrentUserService
{
    public Guid? UserId => null;
    public string? UserName => null;
    public string? Email => null;
    public bool IsAuthenticated => false;
    public IReadOnlyList<string> Roles { get; } = Array.Empty<string>();
    public bool IsInRole(string role) => false;
    public string? IpAddress => null;
    public string? UserAgent => null;
}
