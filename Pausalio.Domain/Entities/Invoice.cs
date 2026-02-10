using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Domain.Entities
{
    public class Invoice
    {
        public Guid Id { get; set; }
        public Guid BusinessProfileId { get; set; }
        public BusinessProfile BusinessProfile { get; set; } = null!;
        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public string InvoiceNumber { get; set; } = null!;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public decimal AmountToPay { get; set; } = 0;
        public DateTime IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public InvoiceStatus InvoiceStatus { get; set; } = InvoiceStatus.Draft;
        public Currency Currency { get; set; } = Currency.RSD;
        public decimal TotalAmount { get; set; } = 0;
        public decimal TotalAmountRSD { get; set; } = 0;
        public decimal ExchangeRate { get; set; } = 1;
        public string? ReferenceNumber { get; set; } = null!;
        public string? Notes { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
