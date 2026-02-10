using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Application.DTOs.Payment;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Expense
{
    public class ExpenseToReturnDto
    {
        public Guid Id { get; set; }
        public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;
        public string Name { get; set; } = null!;
        public string? ReferenceNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public PaymentToReturnDto? Payment { get; set; }
    }
}
