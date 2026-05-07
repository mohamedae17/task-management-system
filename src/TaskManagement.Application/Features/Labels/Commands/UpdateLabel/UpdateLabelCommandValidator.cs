using FluentValidation;

namespace TaskManagement.Application.Features.Labels.Commands.UpdateLabel;

public sealed class UpdateLabelCommandValidator : AbstractValidator<UpdateLabelCommand>
{
    public UpdateLabelCommandValidator()
    {
        RuleFor(c => c.LabelId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(80);
        RuleFor(c => c.ColorHex)
            .NotEmpty()
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{8})$")
            .WithMessage("Color must be a #RRGGBB or #RRGGBBAA hex string.");
        RuleFor(c => c.Description).MaximumLength(500);
    }
}
