using Pausalio.Application.DTOs.InvoiceItem;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Invoice
{
    public class UpdateInvoiceDto
    {
        public Guid ClientId { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public DateTime? DueDate { get; set; }
        public InvoiceStatus InvoiceStatus { get; set; } = InvoiceStatus.Draft;
        public Currency Currency { get; set; } = Currency.RSD;
        public decimal ExchangeRate { get; set; } = 1;
        public string? Notes { get; set; }
        public ICollection<AddInvoiceItemDto> Items { get; set; } = new List<AddInvoiceItemDto>();
    }
}
