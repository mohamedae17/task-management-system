using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence.Configurations;

public sealed class TaskLabelConfiguration : IEntityTypeConfiguration<TaskLabel>
{
    public void Configure(EntityTypeBuilder<TaskLabel> b)
    {
        b.ToTable("TaskLabels");
        b.HasKey(tl => new { tl.TaskId, tl.LabelId });

        b.HasOne(tl => tl.Task)
            .WithMany(t => t.TaskLabels)
            .HasForeignKey(tl => tl.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(tl => tl.Label)
            .WithMany(l => l.TaskLabels)
            .HasForeignKey(tl => tl.LabelId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(tl => tl.LabelId);

        // Mirror the soft-delete filters of both endpoints so EF doesn't surface dangling links.
        b.HasQueryFilter(tl => !tl.Task.IsDeleted && !tl.Label.IsDeleted);
    }
}
