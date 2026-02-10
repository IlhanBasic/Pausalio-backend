using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Application.DTOs.Payment;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.TaxObligation
{
    public class TaxObligationToReturnDto
    {
        public Guid Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public TaxObligationStatus Status { get; set; } = TaxObligationStatus.Pending;
        public DateTime DueDate { get; set; }
        public string? ReferenceNumber { get; set; } = null!;
        public TaxObligationType Type { get; set; } = TaxObligationType.VAT;
        public decimal TotalAmount { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public PaymentToReturnDto? Payment { get; set; }
    }
}
