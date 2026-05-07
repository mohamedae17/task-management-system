using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TaskManagement.Api.Hubs;

[Authorize]
public sealed class NotificationsHub : Hub
{
    public const string Path = "/hubs/notifications";

    public override async Task OnConnectedAsync()
    {
        var userId = ResolveUserId();
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupForUser(userId));

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = ResolveUserId();
        if (userId is not null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupForUser(userId));

        await base.OnDisconnectedAsync(exception);
    }

    public Task SubscribeToTask(Guid taskId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupForTask(taskId));

    public Task UnsubscribeFromTask(Guid taskId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupForTask(taskId));

    public static string GroupForUser(string userId) => $"user:{userId}";
    public static string GroupForTask(Guid taskId) => $"task:{taskId:N}";

    private string? ResolveUserId() =>
        Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
