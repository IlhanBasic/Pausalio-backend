using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;
using Pausalio.Shared.Enums;
using System;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class UserBusinessProfileConfiguration : IEntityTypeConfiguration<UserBusinessProfile>
    {
        public void Configure(EntityTypeBuilder<UserBusinessProfile> builder)
        {
            builder.ToTable("UserBusinessProfiles");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Role)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                   .IsRequired();

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.BusinessProfileId);
            builder.HasIndex(x => new { x.UserId, x.BusinessProfileId }).IsUnique();

            builder.HasOne(x => x.User)
                   .WithMany(u => u.UserBusinessProfiles)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.BusinessProfile)
                   .WithMany(b => b.UserBusinessProfiles)
                   .HasForeignKey(x => x.BusinessProfileId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
