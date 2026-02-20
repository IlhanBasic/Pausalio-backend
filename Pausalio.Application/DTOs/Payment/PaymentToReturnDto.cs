using Pausalio.Application.DTOs.BankAccount;
using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Application.DTOs.Expense;
using Pausalio.Application.DTOs.Invoice;
using Pausalio.Application.DTOs.TaxObligation;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Payment
{
    public class PaymentToReturnDto
    {
        public Guid Id { get; set; }
        public PaymentType PaymentType { get; set; } = PaymentType.InvoicePayment;
        public InvoiceToReturnDto? Invoice { get; set; } = null!;
        public TaxObligationToReturnDto? TaxObligation { get; set; }
        public ExpenseToReturnDto? Expense { get; set; }
        public decimal Amount { get; set; } = 0;
        public Currency Currency { get; set; } = Currency.RSD;
        public decimal? ExchangeRate { get; set; }
        public decimal AmountRSD { get; set; } = 0;
        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public BankAccountToReturnDto? BankAccount { get; set; }
    }
}
