using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Expense
{
    public class ExpenseSummaryDto
    {
        public decimal TotalPending { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalArchived { get; set; }
        public int CountPending { get; set; }
        public int CountPaid { get; set; }
        public int CountArchived { get; set; }
    }
}
