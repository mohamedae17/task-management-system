using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;

namespace TaskManagement.Infrastructure.Persistence.Interceptors;

public sealed class AuditingSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public AuditingSaveChangesInterceptor(ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _currentUser = currentUser;
        _clock = clock;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAuditing(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditing(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditing(DbContext? context)
    {
        if (context is null) return;

        var now = _clock.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            ApplyAuditFields(entry, now, userId);
            ApplySoftDelete(entry, now, userId);
        }
    }

    private static void ApplyAuditFields(EntityEntry entry, DateTime now, Guid? userId)
    {
        if (entry.Entity is not IAuditable auditable) return;

        switch (entry.State)
        {
            case EntityState.Added:
                if (auditable.CreatedAt == default) auditable.CreatedAt = now;
                if (auditable.CreatedBy is null) auditable.CreatedBy = userId;
                break;
            case EntityState.Modified:
                auditable.UpdatedAt = now;
                auditable.UpdatedBy = userId;
                break;
        }
    }

    private static void ApplySoftDelete(EntityEntry entry, DateTime now, Guid? userId)
    {
        if (entry.Entity is not ISoftDelete soft) return;
        if (entry.State != EntityState.Deleted) return;

        entry.State = EntityState.Modified;
        soft.IsDeleted = true;
        soft.DeletedAt = now;
        soft.DeletedBy = userId;
    }
}
