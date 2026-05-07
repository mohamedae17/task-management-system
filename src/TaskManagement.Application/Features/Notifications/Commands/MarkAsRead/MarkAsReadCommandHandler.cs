using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Features.Notifications.Commands.MarkAsRead;

public sealed class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public MarkAsReadCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Unit> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedException();

        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Notification), request.NotificationId);

        if (notification.RecipientId != userId)
            throw new ForbiddenAccessException();

        if (notification.IsRead)
            return Unit.Value;

        notification.IsRead = true;
        notification.ReadAt = _clock.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
