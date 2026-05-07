using FluentValidation;
using TaskManagement.Domain.Constants;

namespace TaskManagement.Application.Features.Users.Commands.SetUserRoles;

public sealed class SetUserRolesCommandValidator : AbstractValidator<SetUserRolesCommand>
{
    public SetUserRolesCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Roles)
            .NotNull()
            .Must(roles => roles.Count > 0)
            .WithMessage("At least one role is required.")
            .Must(roles => roles.All(r => Roles.All.Contains(r)))
            .WithMessage($"Roles must be one of: {string.Join(", ", Roles.All)}.");
    }
}
