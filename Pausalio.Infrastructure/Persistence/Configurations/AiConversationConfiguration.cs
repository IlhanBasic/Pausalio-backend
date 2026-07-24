using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation>
    {
        public void Configure(EntityTypeBuilder<AiConversation> builder)
        {
            builder.ToTable("aiconversations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .HasColumnType("char(36)");

            builder.Property(x => x.BusinessProfileId)
                   .HasColumnType("char(36)")
                   .IsRequired();

            builder.Property(x => x.UserId)
                   .HasColumnType("char(36)")
                   .IsRequired();

            builder.Property(x => x.Title)
                   .HasMaxLength(200);

            builder.Property(x => x.IsDeleted)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                   .IsRequired();

            builder.Property(x => x.UpdatedAt)
                   .IsRequired(false);

            builder.HasOne(x => x.BusinessProfile)
                   .WithMany()
                   .HasForeignKey(x => x.BusinessProfileId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Messages)
                   .WithOne(m => m.Conversation)
                   .HasForeignKey(m => m.ConversationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}