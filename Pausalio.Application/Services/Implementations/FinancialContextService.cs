using Pausalio.Application.Services.Interfaces;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Implementations
{
    public class FinancialContextService : IFinancialContextService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public FinancialContextService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<string> BuildContextAsync()
        {
            var companyIdString = _currentUserService.GetCompany();
            if (!Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException();

            var currentYear = DateTime.UtcNow.Year;
            var currentMonth = DateTime.UtcNow.Month;

            var invoices = await _unitOfWork.InvoiceRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && !x.IsDeleted);

            var totalInvoicedRSD = invoices.Sum(x => x.TotalAmountRSD);
            var totalPaidRSD = invoices
                .Where(x => x.PaymentStatus == PaymentStatus.Paid)
                .Sum(x => x.TotalAmountRSD);
            var totalUnpaidRSD = invoices
                .Where(x => x.PaymentStatus == PaymentStatus.Unpaid)
                .Sum(x => x.TotalAmountRSD);
            var yearlyIncomeRSD = invoices
                .Where(x => x.IssueDate.Year == currentYear && x.InvoiceStatus != InvoiceStatus.Cancelled)
                .Sum(x => x.TotalAmountRSD);

            var expenses = await _unitOfWork.ExpenseRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && !x.IsDeleted);

            var totalExpenses = expenses.Sum(x => x.Amount);
            var paidExpenses = expenses
                .Where(x => x.Status == ExpenseStatus.Paid)
                .Sum(x => x.Amount);
            var pendingExpenses = expenses
                .Where(x => x.Status == ExpenseStatus.Pending)
                .Sum(x => x.Amount);

            var taxObligations = await _unitOfWork.TaxObligationRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId);

            var pendingTaxes = taxObligations
                .Where(x => x.Status == TaxObligationStatus.Pending)
                .Sum(x => x.TotalAmount);
            var overdueTaxes = taxObligations
                .Where(x => x.Status == TaxObligationStatus.Pending && x.DueDate < DateTime.UtcNow)
                .Sum(x => x.TotalAmount);

            var sb = new StringBuilder();
            sb.AppendLine($"Trenutni datum: {DateTime.UtcNow:dd.MM.yyyy}");
            sb.AppendLine($"Godina: {currentYear}, Mesec: {currentMonth}");
            sb.AppendLine();
            sb.AppendLine("=== FAKTURE ===");
            sb.AppendLine($"Ukupno fakturisano (sve vreme): {totalInvoicedRSD:N0} RSD");
            sb.AppendLine($"Ukupno naplaćeno: {totalPaidRSD:N0} RSD");
            sb.AppendLine($"Ukupno nenaplaćeno: {totalUnpaidRSD:N0} RSD");
            sb.AppendLine($"Prihod u {currentYear}. godini: {yearlyIncomeRSD:N0} RSD");
            sb.AppendLine($"Limit paušalnog statusa: 8.000.000 RSD");
            sb.AppendLine($"Preostalo do limita: {(8_000_000 - yearlyIncomeRSD):N0} RSD");
            sb.AppendLine();
            sb.AppendLine("=== TROŠKOVI ===");
            sb.AppendLine($"Ukupni troškovi: {totalExpenses:N0} RSD");
            sb.AppendLine($"Plaćeni troškovi: {paidExpenses:N0} RSD");
            sb.AppendLine($"Troškovi na čekanju: {pendingExpenses:N0} RSD");
            sb.AppendLine();
            sb.AppendLine("=== PORESKI DUGOVI ===");
            sb.AppendLine($"Ukupno neplaćenih poreza: {pendingTaxes:N0} RSD");
            sb.AppendLine($"Zakašneli porezi: {overdueTaxes:N0} RSD");

            return sb.ToString();
        }
    }
}
