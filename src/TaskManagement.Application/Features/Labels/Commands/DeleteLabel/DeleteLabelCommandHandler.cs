using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Features.Labels.Commands.DeleteLabel;

public sealed class DeleteLabelCommandHandler : IRequestHandler<DeleteLabelCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public DeleteLabelCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(DeleteLabelCommand request, CancellationToken cancellationToken)
    {
        var label = await _db.Labels.FirstOrDefaultAsync(l => l.Id == request.LabelId, cancellationToken)
            ?? throw new NotFoundException(nameof(Label), request.LabelId);

        _db.Labels.Remove(label);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
