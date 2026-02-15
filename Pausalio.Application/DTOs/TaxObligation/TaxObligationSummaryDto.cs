using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.TaxObligation
{
    /// <summary>
    /// DTO za automatsko generisanje godišnjih obaveza
    /// </summary>
    public class GenerateTaxObligationsDto
    {
        public int Year { get; set; }
        public decimal MonthlyAmount { get; set; } // Fiksni mesečni iznos
        public TaxObligationType Type { get; set; } = TaxObligationType.VAT;
        public int DueDayOfMonth { get; set; } = 15; // Datum dospeća (npr. 15. u mesecu)
    }

    /// <summary>
    /// DTO za summary pregled
    /// </summary>
    public class TaxObligationSummaryDto
    {
        public decimal TotalPending { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOverdue { get; set; }
        public int CountPending { get; set; }
        public int CountPaid { get; set; }
        public int CountOverdue { get; set; }
    }
}
