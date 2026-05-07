using MediatR;

namespace TaskManagement.Application.Features.Labels.Commands.DeleteLabel;

public sealed record DeleteLabelCommand(Guid LabelId) : IRequest<Unit>;
