using AutoMapper;
using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Auth.Dtos;
using TaskManagement.Application.Features.Users.Dtos;

namespace TaskManagement.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IJwtService _jwt;
    private readonly IIdentityService _identity;
    private readonly IMapper _mapper;

    public RefreshTokenCommandHandler(IJwtService jwt, IIdentityService identity, IMapper mapper)
    {
        _jwt = jwt;
        _identity = identity;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        JwtTokenResponse tokens;
        try
        {
            tokens = await _jwt.RefreshAsync(request.RefreshToken, request.IpAddress, cancellationToken);
        }
        catch (Exception ex) when (ex is not UnauthorizedException)
        {
            throw new UnauthorizedException("Refresh token is invalid or has expired.");
        }

        // Decode the access token's sub claim to find the user — but the IJwtService also
        // exposes the user identity through the refresh-token row, so we re-fetch via email
        // recorded in the access token. Easier path: extract user directly from the JWT.
        // Since IJwtService.RefreshAsync already validated and rotated the token, we trust
        // it and look up the user by parsing the new access token's sub claim.
        var userId = ExtractUserIdFromAccessToken(tokens.AccessToken);
        var user = await _identity.FindByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException("User no longer exists.");

        if (!user.IsActive)
            throw new ForbiddenAccessException("This account has been deactivated.");

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

    // The JWT is a base64url-encoded "header.payload.signature". We only need the sub claim.
    private static Guid ExtractUserIdFromAccessToken(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3)
            throw new UnauthorizedException("Malformed access token.");

        var payload = Base64UrlDecode(parts[1]);
        var doc = System.Text.Json.JsonDocument.Parse(payload);

        if (doc.RootElement.TryGetProperty("sub", out var sub) &&
            Guid.TryParse(sub.GetString(), out var id))
        {
            return id;
        }

        throw new UnauthorizedException("Access token is missing the subject claim.");
    }

    private static string Base64UrlDecode(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(s));
    }
}
