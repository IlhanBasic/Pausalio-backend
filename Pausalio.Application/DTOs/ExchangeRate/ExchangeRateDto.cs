using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.ExchangeRate
{
    public class ExchangeRateDto
    {
        public string Code { get; set; } = null!;
        public DateTime Date { get; set; }
        public DateTime DateFrom { get; set; }
        public int Number { get; set; }
        public decimal Parity { get; set; }
        public decimal CashBuy { get; set; }
        public decimal CashSell { get; set; }
        public decimal ExchangeBuy { get; set; }
        public decimal ExchangeMiddle { get; set; }
        public decimal ExchangeSell { get; set; }
    }
}
