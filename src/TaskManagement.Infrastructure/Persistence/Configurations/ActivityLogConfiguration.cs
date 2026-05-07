using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence.Configurations;

public sealed class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> b)
    {
        b.ToTable("ActivityLogs");
        b.HasKey(a => a.Id);

        b.Property(a => a.Action).HasConversion<int>();
        b.Property(a => a.EntityType).HasMaxLength(120).IsRequired();
        b.Property(a => a.Metadata).HasMaxLength(4000);
        b.Property(a => a.IpAddress).HasMaxLength(64);
        b.Property(a => a.UserAgent).HasMaxLength(512);

        b.HasOne(a => a.User)
            .WithMany(u => u.ActivityLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(a => a.Task)
            .WithMany(t => t.ActivityLogs)
            .HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(a => a.UserId);
        b.HasIndex(a => a.TaskId);
        b.HasIndex(a => a.EntityType);
        b.HasIndex(a => a.CreatedAt);
        b.HasIndex(a => a.IsDeleted);

        b.HasQueryFilter(a => !a.IsDeleted);
    }
}
