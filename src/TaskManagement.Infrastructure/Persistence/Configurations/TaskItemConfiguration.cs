using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence.Configurations;

public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> b)
    {
        b.ToTable("Tasks");
        b.HasKey(t => t.Id);

        b.Property(t => t.Title).HasMaxLength(200).IsRequired();
        b.Property(t => t.Description).HasMaxLength(4000);
        b.Property(t => t.Status).HasConversion<int>();
        b.Property(t => t.Priority).HasConversion<int>();

        b.HasOne(t => t.Creator)
            .WithMany(u => u.CreatedTasks)
            .HasForeignKey(t => t.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(t => t.Assignee)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(t => t.ParentTask)
            .WithMany(t => t.SubTasks)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(t => t.Status);
        b.HasIndex(t => t.Priority);
        b.HasIndex(t => t.DueDate);
        b.HasIndex(t => t.AssigneeId);
        b.HasIndex(t => t.CreatorId);
        b.HasIndex(t => t.IsDeleted);
        b.HasIndex(t => new { t.Status, t.AssigneeId, t.IsDeleted });

        b.HasQueryFilter(t => !t.IsDeleted);
    }
}
