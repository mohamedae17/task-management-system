using MediatR;

namespace TaskManagement.Application.Features.Auth.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(Guid UserId, string Token) : IRequest<Unit>;
