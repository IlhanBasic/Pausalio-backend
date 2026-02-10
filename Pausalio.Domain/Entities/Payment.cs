using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid BusinessProfileId { get; set; }
        public BusinessProfile BusinessProfile { get; set; } = null!;
        public PaymentType PaymentType { get; set; } = PaymentType.InvoicePayment;
        public Invoice? Invoice { get; set; } = null!;
        public Guid? InvoiceId { get; set; }
        public Guid? TaxObligationId { get; set; }
        public TaxObligation? TaxObligation { get; set; }
        public Guid? ExpenseId { get; set; }
        public Expense? Expense { get; set; }
        public decimal Amount { get; set; } = 0;
        public Currency Currency { get; set; } = Currency.RSD;
        public decimal? ExchangeRate { get; set; }
        public decimal AmountRSD { get; set; } = 0;
        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
