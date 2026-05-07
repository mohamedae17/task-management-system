using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Notifications.Dtos;

namespace TaskManagement.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery, PaginatedList<NotificationDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetMyNotificationsQueryHandler(
        IApplicationDbContext db, ICurrentUserService currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<PaginatedList<NotificationDto>> Handle(
        GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedException();

        var query = _db.Notifications.Where(n => n.RecipientId == userId);

        if (request.UnreadOnly == true)
            query = query.Where(n => !n.IsRead);

        return await PaginatedList<NotificationDto>.CreateAsync(
            query.OrderByDescending(n => n.CreatedAt)
                 .ProjectTo<NotificationDto>(_mapper.ConfigurationProvider),
            request.PageNumber, request.PageSize, cancellationToken);
    }
}
