using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Domain.Entities
{
    public class TaxObligation
    {
        public Guid Id { get; set; }
        public Guid BusinessProfileId { get; set; }
        public BusinessProfile BusinessProfile { get; set; } = null!;
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
        public Guid? PaymentId { get; set; }
        public Payment? Payment { get; set; }
    }
}
