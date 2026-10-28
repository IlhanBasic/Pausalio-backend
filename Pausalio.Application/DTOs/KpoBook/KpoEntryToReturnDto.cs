using Pausalio.Shared.Enums;

namespace Pausalio.Application.DTOs.KpoBook
{
    public class KpoEntryToReturnDto
    {
        public Guid Id { get; set; }
        public int Year { get; set; }
        public int SequenceNumber { get; set; }
        public DateTime EntryDate { get; set; }
        public string Description { get; set; } = null!;
        public decimal ProductIncome { get; set; }
        public decimal ServiceIncome { get; set; }
        public decimal TotalIncome { get; set; }
        public Currency OriginalCurrency { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public KpoEntryType EntryType { get; set; }
        public Guid InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public Guid? OriginalEntryId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
