using Pausalio.Application.DTOs.Expense;
using Pausalio.Application.DTOs.Invoice;
using Pausalio.Application.DTOs.Payment;
using Pausalio.Application.DTOs.TaxObligation;
using Pausalio.Application.Services.Interfaces;

namespace Pausalio.Application.Services.Implementations.AIAssistant
{
    public class AIAssistantDataLoader
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IExpenseService _expenseService;
        private readonly ITaxObligationService _taxObligationService;
        private readonly IPaymentService _paymentService;

        public AIAssistantDataLoader(
            IInvoiceService invoiceService,
            IExpenseService expenseService,
            ITaxObligationService taxObligationService,
            IPaymentService paymentService)
        {
            _invoiceService = invoiceService;
            _expenseService = expenseService;
            _taxObligationService = taxObligationService;
            _paymentService = paymentService;
        }

        public async Task<CachedToolData> LoadAllDataAsync()
        {
            var invoices = await _invoiceService.GetAllAsync();
            var expenses = await _expenseService.GetAllAsync();
            var taxObligations = await _taxObligationService.GetAllAsync();
            var payments = await _paymentService.GetAllAsync();
            var invoiceSummary = await _invoiceService.GetSummaryAsync();
            var expenseSummary = await _expenseService.GetSummaryAsync();

            return new CachedToolData
            {
                Invoices = invoices,
                Expenses = expenses,
                TaxObligations = taxObligations,
                Payments = payments,
                InvoiceSummary = invoiceSummary,
                ExpenseSummary = expenseSummary
            };
        }
    }
}
