using MediatR;
using TaskManagement.Application.Features.Labels.Dtos;

namespace TaskManagement.Application.Features.Labels.Commands.UpdateLabel;

public sealed record UpdateLabelCommand(
    Guid LabelId,
    string Name,
    string ColorHex,
    string? Description) : IRequest<LabelDto>;
