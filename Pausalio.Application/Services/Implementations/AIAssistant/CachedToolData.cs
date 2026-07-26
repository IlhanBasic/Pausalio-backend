using Pausalio.Application.DTOs.Expense;
using Pausalio.Application.DTOs.Invoice;
using Pausalio.Application.DTOs.Payment;
using Pausalio.Application.DTOs.TaxObligation;
using Pausalio.Application.DTOs.Reminder;
using Pausalio.Application.DTOs.Client;
using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Application.DTOs.BankAccount;

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
        public required IEnumerable<ReminderToReturnDto> Reminders { get; init; }
        public required IEnumerable<ClientToReturnDto> Clients { get; init; }
        public required BusinessProfileToReturnDto? BusinessProfile { get; init; }
        public required IEnumerable<BankAccountToReturnDto> BankAccounts { get; init; }
    }
}
