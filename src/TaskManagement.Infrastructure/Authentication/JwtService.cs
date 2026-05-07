using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Models;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Authentication;

public sealed class JwtService : IJwtService
{
    private readonly JwtOptions _options;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly IDateTimeProvider _clock;

    public JwtService(
        IOptions<JwtOptions> options,
        ApplicationDbContext db,
        UserManager<AppUser> userManager,
        IDateTimeProvider clock)
    {
        _options = options.Value;
        _db = db;
        _userManager = userManager;
        _clock = clock;
    }

    public async Task<JwtTokenResponse> IssueTokensAsync(
        AppUser user, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, accessExpires) = CreateAccessToken(user, roles);
        var (refreshTokenPlain, refreshHash, refreshExpires) = CreateRefreshToken();

        await _db.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = refreshExpires,
            CreatedByIp = ipAddress
        }, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return new JwtTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenPlain,
            AccessTokenExpiresAt = accessExpires,
            RefreshTokenExpiresAt = refreshExpires,
            TokenType = "Bearer"
        };
    }

    public async Task<JwtTokenResponse> RefreshAsync(
        string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var hash = HashToken(refreshToken);
        var stored = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == hash, cancellationToken)
            ?? throw new UnauthorizedException("Refresh token not found.");

        if (!stored.IsActive)
        {
            // If a previously-revoked token is reused, treat it as a compromise indicator and
            // revoke the user's entire chain of refresh tokens.
            if (stored.IsRevoked)
                await RevokeAllForUserAsync(stored.UserId, "Reuse of revoked token", cancellationToken);

            throw new UnauthorizedException("Refresh token is no longer active.");
        }

        if (!stored.User.IsActive)
            throw new ForbiddenAccessException("This account has been deactivated.");

        var newTokens = await IssueTokensAsync(stored.User, ipAddress, cancellationToken);

        stored.RevokedAt = _clock.UtcNow;
        stored.RevokedByIp = ipAddress;
        stored.ReasonRevoked = "Rotated";
        stored.ReplacedByTokenHash = HashToken(newTokens.RefreshToken);
        await _db.SaveChangesAsync(cancellationToken);

        return newTokens;
    }

    public async Task RevokeAsync(
        string refreshToken, string? ipAddress = null, string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var hash = HashToken(refreshToken);
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash, cancellationToken);
        if (stored is null || stored.IsRevoked) return;

        stored.RevokedAt = _clock.UtcNow;
        stored.RevokedByIp = ipAddress;
        stored.ReasonRevoked = reason ?? "Revoked";
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllForUserAsync(
        Guid userId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var active = await _db.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null && r.ExpiresAt > _clock.UtcNow)
            .ToListAsync(cancellationToken);

        if (active.Count == 0) return;

        var now = _clock.UtcNow;
        foreach (var t in active)
        {
            t.RevokedAt = now;
            t.ReasonRevoked = reason ?? "Bulk revoke";
        }
        await _db.SaveChangesAsync(cancellationToken);
    }

    private (string Token, DateTime ExpiresAt) CreateAccessToken(AppUser user, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = _clock.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? user.Email ?? user.Id.ToString()),
            new("firstName", user.FirstName),
            new("lastName", user.LastName)
        };
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: _clock.UtcNow,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    private (string PlainToken, string Hash, DateTime ExpiresAt) CreateRefreshToken()
    {
        Span<byte> bytes = stackalloc byte[64];
        RandomNumberGenerator.Fill(bytes);
        var plain = Convert.ToBase64String(bytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
        var hash = HashToken(plain);
        return (plain, hash, _clock.UtcNow.AddDays(_options.RefreshTokenDays));
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
