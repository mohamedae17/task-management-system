namespace TaskManagement.Application.Features.Users.Dtos;

/// Lightweight projection used in dropdowns and assignment pickers.
public sealed class UserSummaryDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? AvatarUrl { get; init; }
}
