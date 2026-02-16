using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Payment
{
    public class AddPaymentDto
    {
        public PaymentType PaymentType { get; set; } = PaymentType.InvoicePayment;
        public Guid EntityId { get; set; }
        public decimal Amount { get; set; } = 0;
        public Currency Currency { get; set; } = Currency.RSD;
        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }
    }
}
