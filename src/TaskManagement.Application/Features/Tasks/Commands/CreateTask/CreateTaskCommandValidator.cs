using FluentValidation;

namespace TaskManagement.Application.Features.Tasks.Commands.CreateTask;

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Description).MaximumLength(4000);
        RuleFor(c => c.Status).IsInEnum();
        RuleFor(c => c.Priority).IsInEnum();
        RuleFor(c => c.DueDate)
            .Must(d => d == null || d > DateTime.UtcNow.AddYears(-1))
            .WithMessage("Due date may not be more than a year in the past.");
    }
}
