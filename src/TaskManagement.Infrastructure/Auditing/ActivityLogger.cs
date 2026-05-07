using System.Text.Json;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Auditing;

public sealed class ActivityLogger : IActivityLogger
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ActivityLogger(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task LogAsync(
        ActivityAction action, string entityType, Guid? entityId,
        Guid? taskId = null, object? metadata = null, CancellationToken cancellationToken = default)
    {
        var log = new ActivityLog
        {
            UserId = _currentUser.UserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            TaskId = taskId,
            Metadata = metadata is null ? null : JsonSerializer.Serialize(metadata, JsonOpts),
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent
        };

        await _db.ActivityLogs.AddAsync(log, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
