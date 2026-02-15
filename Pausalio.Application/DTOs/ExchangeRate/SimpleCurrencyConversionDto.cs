using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.ExchangeRate
{
    public class SimpleCurrencyConversionDto
    {
        public string FromCurrency { get; set; } = null!;
        public string ToCurrency { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime Date { get; set; }
    }
}
