using FluentValidation;

namespace TaskManagement.Application.Features.Tasks.Commands.ChangeTaskStatus;

public sealed class ChangeTaskStatusCommandValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    public ChangeTaskStatusCommandValidator()
    {
        RuleFor(c => c.TaskId).NotEmpty();
        RuleFor(c => c.Status).IsInEnum();
    }
}
