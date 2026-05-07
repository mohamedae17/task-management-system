using TaskManagement.Application.Common.Models;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Common.Interfaces;

public interface IJwtService
{
    Task<JwtTokenResponse> IssueTokensAsync(
        AppUser user,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    Task<JwtTokenResponse> RefreshAsync(
        string refreshToken,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        string refreshToken,
        string? ipAddress = null,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task RevokeAllForUserAsync(
        Guid userId,
        string? reason = null,
        CancellationToken cancellationToken = default);
}
