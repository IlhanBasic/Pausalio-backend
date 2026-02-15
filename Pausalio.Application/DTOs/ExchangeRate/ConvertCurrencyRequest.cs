using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.ExchangeRate
{
    public class ConvertCurrencyRequest
    {
        public Currency FromCurrency { get; set; }
        public Currency ToCurrency { get; set; } = Currency.RSD;
        public decimal Amount { get; set; }
    }
}
