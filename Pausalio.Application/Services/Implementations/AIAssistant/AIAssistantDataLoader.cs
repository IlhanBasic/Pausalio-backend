using Pausalio.Application.DTOs.Expense;
using Pausalio.Application.DTOs.Invoice;
using Pausalio.Application.DTOs.Payment;
using Pausalio.Application.DTOs.TaxObligation;
using Pausalio.Application.DTOs.Reminder;
using Pausalio.Application.DTOs.Client;
using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Application.DTOs.BankAccount;
using Pausalio.Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Implementations.AIAssistant
{
    public class AIAssistantDataLoader
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IExpenseService _expenseService;
        private readonly ITaxObligationService _taxObligationService;
        private readonly IPaymentService _paymentService;
        private readonly IReminderService _reminderService;
        private readonly IClientService _clientService;
        private readonly IBankAccountService _bankAccountService;
        private readonly IBusinessProfileService _businessProfileService;
        private readonly ICurrentUserService _currentUserService;

        public AIAssistantDataLoader(
            IInvoiceService invoiceService,
            IExpenseService expenseService,
            ITaxObligationService taxObligationService,
            IPaymentService paymentService,
            IReminderService reminderService,
            IClientService clientService,
            IBankAccountService bankAccountService,
            IBusinessProfileService businessProfileService,
            ICurrentUserService currentUserService)
        {
            _invoiceService = invoiceService;
            _expenseService = expenseService;
            _taxObligationService = taxObligationService;
            _paymentService = paymentService;
            _reminderService = reminderService;
            _clientService = clientService;
            _bankAccountService = bankAccountService;
            _businessProfileService = businessProfileService;
            _currentUserService = currentUserService;
        }

        public async Task<CachedToolData> LoadAllDataAsync()
        {
            var invoices = await _invoiceService.GetAllAsync();
            var expenses = await _expenseService.GetAllAsync();
            var taxObligations = await _taxObligationService.GetAllAsync();
            var payments = await _paymentService.GetAllAsync();
            var invoiceSummary = await _invoiceService.GetSummaryAsync();
            var expenseSummary = await _expenseService.GetSummaryAsync();

            var reminders = await _reminderService.GetAllAsync();
            var clients = await _clientService.GetAllAsync();
            var bankAccounts = await _bankAccountService.GetAllAsync();

            BusinessProfileToReturnDto? businessProfile = null;
            var companyIdString = _currentUserService.GetCompany();
            if (Guid.TryParse(companyIdString, out var companyId))
            {
                businessProfile = await _businessProfileService.GetByIdAsync(companyId);
            }

            return new CachedToolData
            {
                Invoices = invoices,
                Expenses = expenses,
                TaxObligations = taxObligations,
                Payments = payments,
                InvoiceSummary = invoiceSummary,
                ExpenseSummary = expenseSummary,
                Reminders = reminders,
                Clients = clients,
                BankAccounts = bankAccounts,
                BusinessProfile = businessProfile
            };
        }
    }
}
