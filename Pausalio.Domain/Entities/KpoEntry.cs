using Pausalio.Shared.Enums;

namespace Pausalio.Domain.Entities
{
    /// <summary>
    /// Zapis u KPO knjizi (Knjiga o ostvarenom prometu paušalno oporezovanih obveznika).
    /// Append-only: jednom kreiran zapis se ne menja i ne briše.
    /// Korekcije se vrše dodavanjem storno zapisa (EntryType = Storno).
    /// </summary>
    public class KpoEntry
    {
        public Guid Id { get; set; }

        public Guid BusinessProfileId { get; set; }
        public BusinessProfile BusinessProfile { get; set; } = null!;

        // Kolona 1: Redni broj — kontinuiran od 1, bez praznina, resetuje se svake godine
        public int Year { get; set; }
        public int SequenceNumber { get; set; }

        // Kolona 2: Datum i opis knjiženja (sa brojem fakture)
        public DateTime EntryDate { get; set; }
        public string Description { get; set; } = null!;

        // Kolona 3: Prihod od prodaje proizvoda
        public decimal ProductIncome { get; set; }

        // Kolona 4: Prihod od izvršenih usluga
        public decimal ServiceIncome { get; set; }

        // Kolona 5: Svega = kolona 3 + kolona 4
        public decimal TotalIncome { get; set; }

        // Valutne informacije (za devizne fakture — originalna valuta, iznos i kurs)
        public Currency OriginalCurrency { get; set; } = Currency.RSD;
        public decimal OriginalAmount { get; set; }
        public decimal ExchangeRate { get; set; } = 1;

        // Tip zapisa
        public KpoEntryType EntryType { get; set; } = KpoEntryType.Regular;

        // Veza sa fakturom
        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        // Storno veza — ako je ovo storno, pokazuje na originalni zapis
        public Guid? OriginalEntryId { get; set; }
        public KpoEntry? OriginalEntry { get; set; }

        // Audit (samo kreiranje, jer je append-only)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
