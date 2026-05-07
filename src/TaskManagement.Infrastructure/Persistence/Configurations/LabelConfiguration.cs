using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence.Configurations;

public sealed class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> b)
    {
        b.ToTable("Labels");
        b.HasKey(l => l.Id);

        b.Property(l => l.Name).HasMaxLength(80).IsRequired();
        b.Property(l => l.ColorHex).HasMaxLength(9).IsRequired();
        b.Property(l => l.Description).HasMaxLength(500);

        b.HasIndex(l => l.Name).IsUnique().HasFilter("[IsDeleted] = 0");
        b.HasIndex(l => l.IsDeleted);

        b.HasQueryFilter(l => !l.IsDeleted);
    }
}
