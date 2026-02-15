using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Invoice
{
    public class InvoiceSummaryDto
    {
        public decimal TotalDraft { get; set; }
        public decimal TotalSent { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalUnpaid { get; set; }
        public decimal TotalOverdue { get; set; }
        public int CountDraft { get; set; }
        public int CountSent { get; set; }
        public int CountPaid { get; set; }
        public int CountUnpaid { get; set; }
        public int CountOverdue { get; set; }
    }
}
