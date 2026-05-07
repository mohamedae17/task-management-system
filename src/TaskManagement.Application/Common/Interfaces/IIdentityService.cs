using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Common.Interfaces;

public sealed record IdentityResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static IdentityResult Success() => new(true, Array.Empty<string>());
    public static IdentityResult Failure(params string[] errors) => new(false, errors);
}

public interface IIdentityService
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<(IdentityResult Result, AppUser? User)> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string role,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, AppUser? User)> ValidateCredentialsAsync(
        string emailOrUsername,
        string password,
        CancellationToken cancellationToken = default);

    Task<AppUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AppUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRolesAsync(AppUser user);
    Task<IdentityResult> SetRolesAsync(AppUser user, IEnumerable<string> roles);

    Task<string> GenerateEmailConfirmationTokenAsync(AppUser user);
    Task<IdentityResult> ConfirmEmailAsync(AppUser user, string token);

    Task<string> GeneratePasswordResetTokenAsync(AppUser user);
    Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string newPassword);

    Task<IdentityResult> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword);

    Task<IdentityResult> UpdateProfileAsync(AppUser user, string firstName, string lastName, string? avatarUrl);

    Task SetActiveAsync(AppUser user, bool isActive, CancellationToken cancellationToken = default);
    Task UpdateLastLoginAsync(AppUser user, CancellationToken cancellationToken = default);
}
