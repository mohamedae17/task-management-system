using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;
using AppIdentityResult = TaskManagement.Application.Common.Interfaces.IdentityResult;

namespace TaskManagement.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public IdentityService(
        UserManager<AppUser> userManager,
        ApplicationDbContext db,
        IDateTimeProvider clock)
    {
        _userManager = userManager;
        _db = db;
        _clock = clock;
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        _userManager.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public async Task<(AppIdentityResult Result, AppUser? User)> CreateUserAsync(
        string email, string password, string firstName, string lastName, string role,
        CancellationToken cancellationToken = default)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = false,
            IsActive = true
        };

        var create = await _userManager.CreateAsync(user, password);
        if (!create.Succeeded)
            return (AppIdentityResult.Failure(create.Errors.Select(e => e.Description).ToArray()), null);

        var addRole = await _userManager.AddToRoleAsync(user, role);
        if (!addRole.Succeeded)
            return (AppIdentityResult.Failure(addRole.Errors.Select(e => e.Description).ToArray()), null);

        return (AppIdentityResult.Success(), user);
    }

    public async Task<(bool Succeeded, AppUser? User)> ValidateCredentialsAsync(
        string emailOrUsername, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(emailOrUsername)
            ?? await _userManager.FindByNameAsync(emailOrUsername);

        if (user is null) return (false, null);

        if (await _userManager.IsLockedOutAsync(user)) return (false, null);

        var ok = await _userManager.CheckPasswordAsync(user, password);
        if (!ok)
        {
            await _userManager.AccessFailedAsync(user);
            return (false, null);
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        return (true, user);
    }

    public Task<AppUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _userManager.FindByIdAsync(userId.ToString());

    public Task<AppUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _userManager.FindByEmailAsync(email);

    public async Task<IReadOnlyList<string>> GetRolesAsync(AppUser user) =>
        (await _userManager.GetRolesAsync(user)).ToList();

    public async Task<AppIdentityResult> SetRolesAsync(AppUser user, IEnumerable<string> roles)
    {
        var current = await _userManager.GetRolesAsync(user);
        var desired = roles.Distinct().ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toRemove = current.Where(r => !desired.Contains(r)).ToList();
        var toAdd = desired.Where(r => !current.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();

        if (toRemove.Count > 0)
        {
            var rm = await _userManager.RemoveFromRolesAsync(user, toRemove);
            if (!rm.Succeeded)
                return AppIdentityResult.Failure(rm.Errors.Select(e => e.Description).ToArray());
        }
        if (toAdd.Count > 0)
        {
            var add = await _userManager.AddToRolesAsync(user, toAdd);
            if (!add.Succeeded)
                return AppIdentityResult.Failure(add.Errors.Select(e => e.Description).ToArray());
        }
        return AppIdentityResult.Success();
    }

    public Task<string> GenerateEmailConfirmationTokenAsync(AppUser user) =>
        _userManager.GenerateEmailConfirmationTokenAsync(user);

    public async Task<AppIdentityResult> ConfirmEmailAsync(AppUser user, string token)
    {
        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded
            ? AppIdentityResult.Success()
            : AppIdentityResult.Failure(result.Errors.Select(e => e.Description).ToArray());
    }

    public Task<string> GeneratePasswordResetTokenAsync(AppUser user) =>
        _userManager.GeneratePasswordResetTokenAsync(user);

    public async Task<AppIdentityResult> ResetPasswordAsync(AppUser user, string token, string newPassword)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded
            ? AppIdentityResult.Success()
            : AppIdentityResult.Failure(result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<AppIdentityResult> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword)
    {
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded
            ? AppIdentityResult.Success()
            : AppIdentityResult.Failure(result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<AppIdentityResult> UpdateProfileAsync(
        AppUser user, string firstName, string lastName, string? avatarUrl)
    {
        user.FirstName = firstName;
        user.LastName = lastName;
        user.AvatarUrl = avatarUrl;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? AppIdentityResult.Success()
            : AppIdentityResult.Failure(result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task SetActiveAsync(AppUser user, bool isActive, CancellationToken cancellationToken = default)
    {
        user.IsActive = isActive;
        await _userManager.UpdateAsync(user);
    }

    public async Task UpdateLastLoginAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        user.LastLoginAt = _clock.UtcNow;
        await _userManager.UpdateAsync(user);
    }
}
