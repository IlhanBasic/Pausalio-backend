using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Application.DTOs.Client;
using Pausalio.Application.DTOs.InvoiceItem;
using Pausalio.Application.DTOs.Payment;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Invoice
{
    public class InvoiceToReturnDto
    {
        public Guid Id { get; set; }
        public BusinessProfileToReturnDto BusinessProfile { get; set; } = null!;
        public ClientToReturnDto Client { get; set; } = null!;
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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<InvoiceItemToReturnDto> Items { get; set; } = new List<InvoiceItemToReturnDto>();
        public ICollection<PaymentToReturnDto> Payments { get; set; } = new List<PaymentToReturnDto>();
    }
}
