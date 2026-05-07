using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence.Configurations;

public sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> b)
    {
        b.ToTable("Attachments");
        b.HasKey(a => a.Id);

        b.Property(a => a.FileName).HasMaxLength(260).IsRequired();
        b.Property(a => a.ContentType).HasMaxLength(120).IsRequired();
        b.Property(a => a.StoragePath).HasMaxLength(1024).IsRequired();

        b.HasOne(a => a.Task)
            .WithMany(t => t.Attachments)
            .HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(a => a.UploadedBy)
            .WithMany(u => u.UploadedAttachments)
            .HasForeignKey(a => a.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(a => a.TaskId);
        b.HasIndex(a => a.IsDeleted);

        b.HasQueryFilter(a => !a.IsDeleted);
    }
}
