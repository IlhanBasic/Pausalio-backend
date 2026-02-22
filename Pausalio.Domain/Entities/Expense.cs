using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Domain.Entities
{
    public class Expense
    {
        public Guid Id { get; set; }
        public Guid BusinessProfileId { get; set; }
        public BusinessProfile BusinessProfile { get; set; } = null!;
        public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;
        public string Name { get; set; } = null!;
        public string? ReferenceNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
