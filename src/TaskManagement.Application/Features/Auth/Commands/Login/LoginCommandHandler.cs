using AutoMapper;
using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Auth.Dtos;
using TaskManagement.Application.Features.Users.Dtos;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IIdentityService _identity;
    private readonly IJwtService _jwt;
    private readonly IActivityLogger _activity;
    private readonly IMapper _mapper;

    public LoginCommandHandler(
        IIdentityService identity,
        IJwtService jwt,
        IActivityLogger activity,
        IMapper mapper)
    {
        _identity = identity;
        _jwt = jwt;
        _activity = activity;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, user) = await _identity.ValidateCredentialsAsync(
            request.Email, request.Password, cancellationToken);

        if (!succeeded || user is null)
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
            throw new ForbiddenAccessException("This account has been deactivated.");

        var tokens = await _jwt.IssueTokensAsync(user, request.IpAddress, cancellationToken);
        await _identity.UpdateLastLoginAsync(user, cancellationToken);
        await _activity.LogAsync(ActivityAction.Login, "User", user.Id, cancellationToken: cancellationToken);

        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = await _identity.GetRolesAsync(user);

        return new AuthResponseDto
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            AccessTokenExpiresAt = tokens.AccessTokenExpiresAt,
            RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt,
            User = dto
        };
    }
}
