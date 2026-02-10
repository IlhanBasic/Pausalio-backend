using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Expense
{
    public class UpdateExpenseDto
    {
        public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;
        public string Name { get; set; } = null!;
        public decimal Amount { get; set; }
    }
}
