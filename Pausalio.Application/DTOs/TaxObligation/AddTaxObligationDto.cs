using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.TaxObligation
{
    public class AddTaxObligationDto
    {
        public DateTime DueDate { get; set; }
        public TaxObligationType Type { get; set; } = TaxObligationType.VAT;
        public decimal TotalAmount { get; set; }
    }
}
