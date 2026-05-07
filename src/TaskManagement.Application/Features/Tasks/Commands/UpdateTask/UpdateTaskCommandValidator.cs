using FluentValidation;

namespace TaskManagement.Application.Features.Tasks.Commands.UpdateTask;

public sealed class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(c => c.TaskId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Description).MaximumLength(4000);
        RuleFor(c => c.Status).IsInEnum();
        RuleFor(c => c.Priority).IsInEnum();
    }
}
