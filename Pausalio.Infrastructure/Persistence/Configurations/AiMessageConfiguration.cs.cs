using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class AiMessageConfiguration : IEntityTypeConfiguration<AiMessage>
    {
        public void Configure(EntityTypeBuilder<AiMessage> builder)
        {
            builder.ToTable("aimessages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .HasColumnType("char(36)");

            builder.Property(x => x.ConversationId)
                   .HasColumnType("char(36)")
                   .IsRequired();

            builder.Property(x => x.Role)
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.Content)
                   .HasColumnType("longtext");

            builder.Property(x => x.PromptTokens)
                   .IsRequired(false);

            builder.Property(x => x.CompletionTokens)
                   .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                   .IsRequired();

            builder.HasMany(x => x.ToolCalls)
                   .WithOne(t => t.Message)
                   .HasForeignKey(t => t.MessageId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}