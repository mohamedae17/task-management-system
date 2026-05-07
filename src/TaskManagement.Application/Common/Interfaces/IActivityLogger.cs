using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Common.Interfaces;

public interface IActivityLogger
{
    Task LogAsync(
        ActivityAction action,
        string entityType,
        Guid? entityId,
        Guid? taskId = null,
        object? metadata = null,
        CancellationToken cancellationToken = default);
}
