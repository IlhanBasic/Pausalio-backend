using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Payment
{
    public class PaymentSummaryDto
    {
        public decimal TotalInvoicePayments { get; set; }
        public decimal TotalTaxPayments { get; set; }
        public decimal TotalExpensePayments { get; set; }
        public int CountInvoicePayments { get; set; }
        public int CountTaxPayments { get; set; }
        public int CountExpensePayments { get; set; }
    }
}
