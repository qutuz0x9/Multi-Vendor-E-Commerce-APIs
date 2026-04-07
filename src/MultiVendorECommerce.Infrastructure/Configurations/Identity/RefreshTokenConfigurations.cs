using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendorECommerce.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiVendorECommerce.Infrastructure.Configurations.Identity
{
    public class RefreshTokenConfigurations : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens", "identity");

            builder.HasKey(rt => rt.Id);
            builder.Property(rt => rt.Id).HasColumnName("Id").HasDefaultValueSql("gen_random_uuid()").IsRequired();
            builder.Property(rt => rt.Token).HasColumnName("Token").IsRequired();
            builder.Property(rt => rt.UserId).HasColumnName("UserId").IsRequired();
            builder.Property(rt => rt.ExpiresAt).HasColumnName("ExpiresAt").IsRequired();
            builder.Property(rt => rt.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(rt => rt.ModifiedAt).HasColumnName("ModifiedAt");
            builder.Property(rt => rt.RevokedAt).HasColumnName("RevokedAt");
            builder.Property(rt => rt.ReplacedByToken).HasColumnName("ReplacedByToken");
            builder.Property(rt => rt.IsRevoked).HasColumnName("IsRevoked").HasDefaultValue(false);
            builder.Property(rt => rt.IsUsed).HasColumnName("IsUsed").HasDefaultValue(false);
            builder.Property(rt => rt.IpAddress).HasColumnName("IpAddress");
            builder.Property(rt => rt.UserAgent).HasColumnName("UserAgent");


            // Indexes
            builder.HasIndex(rt => rt.UserId).HasDatabaseName("IX_RefreshTokens_UserId");

            // Relationships
            builder.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
