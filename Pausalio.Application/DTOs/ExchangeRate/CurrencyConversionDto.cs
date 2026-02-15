using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.ExchangeRate
{
    public class CurrencyConversionDto
    {
        public ExchangeRateDto Rate { get; set; } = null!;
        public decimal BuyMiddle { get; set; }
        public decimal SellMiddle { get; set; }
        public decimal BuyExchange { get; set; }
        public decimal SellExchange { get; set; }
        public decimal BuyCash { get; set; }
        public decimal SellCash { get; set; }
    }
}
