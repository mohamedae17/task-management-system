using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("RefreshTokens");
        b.HasKey(r => r.Id);

        b.Property(r => r.TokenHash).HasMaxLength(512).IsRequired();
        b.Property(r => r.ReplacedByTokenHash).HasMaxLength(512);
        b.Property(r => r.CreatedByIp).HasMaxLength(64);
        b.Property(r => r.RevokedByIp).HasMaxLength(64);
        b.Property(r => r.ReasonRevoked).HasMaxLength(256);

        b.Ignore(r => r.IsActive);
        b.Ignore(r => r.IsExpired);
        b.Ignore(r => r.IsRevoked);

        b.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(r => r.TokenHash).IsUnique();
        b.HasIndex(r => r.UserId);
        b.HasIndex(r => r.ExpiresAt);
        b.HasIndex(r => r.IsDeleted);

        b.HasQueryFilter(r => !r.IsDeleted);
    }
}
