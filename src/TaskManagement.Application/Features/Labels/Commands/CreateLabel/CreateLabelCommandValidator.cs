using FluentValidation;

namespace TaskManagement.Application.Features.Labels.Commands.CreateLabel;

public sealed class CreateLabelCommandValidator : AbstractValidator<CreateLabelCommand>
{
    public CreateLabelCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(80);
        RuleFor(c => c.ColorHex)
            .NotEmpty()
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{8})$")
            .WithMessage("Color must be a #RRGGBB or #RRGGBBAA hex string.");
        RuleFor(c => c.Description).MaximumLength(500);
    }
}
