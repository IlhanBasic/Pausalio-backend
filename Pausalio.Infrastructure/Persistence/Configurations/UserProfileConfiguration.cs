using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;
using System;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("UserProfiles");

            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Role)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(x => x.FirstName)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(x => x.LastName)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(x => x.Email)
                   .HasMaxLength(150)
                   .IsRequired();

            builder.Property(x => x.PasswordHash)
                   .HasMaxLength(500)
                   .IsRequired();

            builder.Property(x => x.ProfilePicture)
                   .HasMaxLength(500);

            builder.Property(x => x.Phone)
                   .HasMaxLength(20);

            builder.Property(x => x.City)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(x => x.Address)
                   .HasMaxLength(250);

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .HasDefaultValueSql("UTC_TIMESTAMP()")
                   .IsRequired();

            builder.Property(x => x.UpdatedAt)
                   .IsRequired(false);

            builder.HasIndex(x => x.Email)
                   .IsUnique();

            builder.HasMany(x => x.UserBusinessProfiles)
                   .WithOne(ubp => ubp.User)
                   .HasForeignKey(ubp => ubp.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.IsEmailVerified)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(x => x.EmailVerificationToken)
                   .HasMaxLength(500);

            builder.Property(x => x.PasswordResetToken)
                   .HasMaxLength(500);
        }
    }
}
