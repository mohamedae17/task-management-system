using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence.Configurations;

public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        b.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        b.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        b.Property(u => u.AvatarUrl).HasMaxLength(500);

        b.HasIndex(u => u.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
        b.HasIndex(u => u.IsDeleted);
        b.HasIndex(u => u.IsActive);

        b.HasQueryFilter(u => !u.IsDeleted);
    }
}
