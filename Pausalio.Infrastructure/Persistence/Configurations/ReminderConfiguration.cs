using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;
using System;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
    {
        public void Configure(EntityTypeBuilder<Reminder> builder)
        {
            builder.ToTable("Reminders");
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Reminders_IsCompleted", "IsCompleted IN (0,1)");
                t.HasCheckConstraint("CK_Reminders_IsDeleted", "IsDeleted IN (0,1)");
                t.HasCheckConstraint("CK_Reminders_Dates", "DeletedAt IS NULL OR DeletedAt >= CreatedAt");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(x => x.Description)
                   .HasMaxLength(500);

            builder.Property(x => x.ReminderType)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.DueDate)
                   .IsRequired();

            builder.Property(x => x.IsCompleted)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(x => x.CompletedAt)
                   .IsRequired(false);

            builder.Property(x => x.IsDeleted)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(x => x.DeletedAt)
                   .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                   .HasDefaultValueSql("UTC_TIMESTAMP()")
                   .IsRequired();

            builder.HasIndex(x => x.BusinessProfileId);
            builder.HasIndex(x => new { x.BusinessProfileId, x.IsCompleted });
            builder.HasIndex(x => new { x.BusinessProfileId, x.IsDeleted });

            builder.HasOne(x => x.BusinessProfile)
                   .WithMany(x => x.Reminders)
                   .HasForeignKey(x => x.BusinessProfileId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
