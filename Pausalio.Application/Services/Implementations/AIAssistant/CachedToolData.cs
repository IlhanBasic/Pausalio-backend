using Pausalio.Application.DTOs.Expense;
using Pausalio.Application.DTOs.Invoice;
using Pausalio.Application.DTOs.Payment;
using Pausalio.Application.DTOs.TaxObligation;

namespace Pausalio.Application.Services.Implementations.AIAssistant
{
    public class CachedToolData
    {
        public required IEnumerable<InvoiceToReturnDto> Invoices { get; init; }
        public required IEnumerable<ExpenseToReturnDto> Expenses { get; init; }
        public required IEnumerable<TaxObligationToReturnDto> TaxObligations { get; init; }
        public required IEnumerable<PaymentToReturnDto> Payments { get; init; }
        public required object InvoiceSummary { get; init; }
        public required object ExpenseSummary { get; init; }
    }
}
