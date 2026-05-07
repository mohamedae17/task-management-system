using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("Notifications");
        b.HasKey(n => n.Id);

        b.Property(n => n.Type).HasConversion<int>();
        b.Property(n => n.Title).HasMaxLength(200).IsRequired();
        b.Property(n => n.Message).HasMaxLength(2000).IsRequired();
        b.Property(n => n.RelatedEntityType).HasMaxLength(80);
        b.Property(n => n.ActionUrl).HasMaxLength(1024);

        b.HasOne(n => n.Recipient)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(n => n.RecipientId);
        b.HasIndex(n => new { n.RecipientId, n.IsRead });
        b.HasIndex(n => n.IsDeleted);

        b.HasQueryFilter(n => !n.IsDeleted);
    }
}
