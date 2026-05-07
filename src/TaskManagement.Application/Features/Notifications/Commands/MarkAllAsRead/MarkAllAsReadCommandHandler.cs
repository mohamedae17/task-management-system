using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Features.Notifications.Commands.MarkAllAsRead;

public sealed class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public MarkAllAsReadCommandHandler(
        IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<int> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedException();

        var unread = await _db.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0) return 0;

        var now = _clock.UtcNow;
        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return unread.Count;
    }
}
