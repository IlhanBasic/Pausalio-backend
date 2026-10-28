using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class KpoEntryConfiguration : IEntityTypeConfiguration<KpoEntry>
    {
        public void Configure(EntityTypeBuilder<KpoEntry> builder)
        {
            builder.ToTable("KpoEntries");

            builder.HasKey(x => x.Id);

            // Kolona 1: Redni broj — unique per business per year, gapless
            builder.Property(x => x.Year)
                .IsRequired();

            builder.Property(x => x.SequenceNumber)
                .IsRequired();

            // Kolona 2: Datum i opis
            builder.Property(x => x.EntryDate)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired();

            // Kolona 3: Prihod od prodaje proizvoda
            builder.Property(x => x.ProductIncome)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            // Kolona 4: Prihod od izvršenih usluga
            builder.Property(x => x.ServiceIncome)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            // Kolona 5: Svega = 3 + 4
            builder.Property(x => x.TotalIncome)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            // Valutne informacije
            builder.Property(x => x.OriginalCurrency)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.OriginalAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(x => x.ExchangeRate)
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(1)
                .IsRequired();

            // Tip zapisa
            builder.Property(x => x.EntryType)
                .HasConversion<int>()
                .IsRequired();

            // Audit
            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            // --- UNIQUE CONSTRAINT: gapless numbering guarantee ---
            builder.HasIndex(x => new { x.BusinessProfileId, x.Year, x.SequenceNumber })
                .IsUnique()
                .HasDatabaseName("IX_KpoEntries_Business_Year_Seq");

            // Index za brzo pretraživanje po fakturi
            builder.HasIndex(x => new { x.BusinessProfileId, x.InvoiceId })
                .HasDatabaseName("IX_KpoEntries_Business_Invoice");

            builder.HasIndex(x => new { x.BusinessProfileId, x.Year })
                .HasDatabaseName("IX_KpoEntries_Business_Year");

            // --- RELATIONSHIPS ---
            builder.HasOne(x => x.BusinessProfile)
                .WithMany(x => x.KpoEntries)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Invoice)
                .WithMany()
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OriginalEntry)
                .WithMany()
                .HasForeignKey(x => x.OriginalEntryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}
