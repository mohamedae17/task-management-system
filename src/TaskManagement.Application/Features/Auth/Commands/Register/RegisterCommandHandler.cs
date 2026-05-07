using AutoMapper;
using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Auth.Dtos;
using TaskManagement.Application.Features.Users.Dtos;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IIdentityService _identity;
    private readonly IJwtService _jwt;
    private readonly IEmailSender _email;
    private readonly IActivityLogger _activity;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(
        IIdentityService identity,
        IJwtService jwt,
        IEmailSender email,
        IActivityLogger activity,
        IMapper mapper)
    {
        _identity = identity;
        _jwt = jwt;
        _email = email;
        _activity = activity;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _identity.EmailExistsAsync(request.Email, cancellationToken))
            throw new ConflictException($"An account with email '{request.Email}' already exists.");

        var (result, user) = await _identity.CreateUserAsync(
            request.Email, request.Password, request.FirstName, request.LastName,
            Roles.Employee, cancellationToken);

        if (!result.Succeeded || user is null)
            throw new BusinessRuleException(string.Join("; ", result.Errors));

        var confirmToken = await _identity.GenerateEmailConfirmationTokenAsync(user);
        await SendVerificationEmailAsync(user.Email!, user.FullName, user.Id, confirmToken, cancellationToken);

        var tokens = await _jwt.IssueTokensAsync(user, request.IpAddress, cancellationToken);
        await _identity.UpdateLastLoginAsync(user, cancellationToken);

        await _activity.LogAsync(ActivityAction.Created, "User", user.Id, cancellationToken: cancellationToken);

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

    private Task SendVerificationEmailAsync(
        string email, string fullName, Guid userId, string token, CancellationToken cancellationToken)
    {
        var body = $@"
<p>Hi {fullName},</p>
<p>Welcome to the Task Management System. Please verify your email by submitting the token below to <code>POST /api/auth/verify-email</code>:</p>
<pre>UserId: {userId}
Token:  {token}</pre>
<p>If you did not create this account, you can safely ignore this message.</p>";

        return _email.SendAsync(new EmailMessage(
            To: email,
            Subject: "Verify your Task Management account",
            HtmlBody: body,
            ToDisplayName: fullName), cancellationToken);
    }
}
