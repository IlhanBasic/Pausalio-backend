using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.BankAccount
{
    public class AddBankAccountDto
    {
        public string BankName { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public Currency Currency { get; set; } = Currency.RSD!;
        public string? IBAN { get; set; }
        public string? SWIFT { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
