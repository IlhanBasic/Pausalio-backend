using Pausalio.Application.DTOs.BankAccount;
using Pausalio.Application.DTOs.Client;
using Pausalio.Application.DTOs.Document;
using Pausalio.Application.DTOs.Expense;
using Pausalio.Application.DTOs.Invoice;
using Pausalio.Application.DTOs.Item;
using Pausalio.Application.DTOs.Payment;
using Pausalio.Application.DTOs.Reminder;
using Pausalio.Application.DTOs.TaxObligation;
using Pausalio.Application.DTOs.UserBusinessProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.BusinessProfile
{
    public class BusinessProfileToReturnDto
    {
        public Guid Id { get; set; }
        public string BusinessName { get; set; } = null!;
        public string PIB { get; set; } = null!;
        public string MB { get; set; } = string.Empty;
        public string ActivityCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? CompanyLogo { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<UserBusinessProfileToReturnDto> UserBusinessProfiles { get; set; } = new List<UserBusinessProfileToReturnDto>();
        public ICollection<BankAccountToReturnDto> BankAccounts { get; set; } = new List<BankAccountToReturnDto>();
        public ICollection<ClientToReturnDto> Clients { get; set; } = new List<ClientToReturnDto>();
        public ICollection<InvoiceToReturnDto> Invoices { get; set; } = new List<InvoiceToReturnDto>();
        public ICollection<PaymentToReturnDto> Payments { get; set; } = new List<PaymentToReturnDto>();
        public ICollection<ExpenseToReturnDto> Expenses { get; set; } = new List<ExpenseToReturnDto>();
        public ICollection<TaxObligationToReturnDto> TaxObligations { get; set; } = new List<TaxObligationToReturnDto>();
        public ICollection<DocumentToReturnDto> Documents { get; set; } = new List<DocumentToReturnDto>();
        public ICollection<ReminderToReturnDto> Reminders { get; set; } = new List<ReminderToReturnDto>();
        public ICollection<ItemToReturnDto> Items { get; set; } = new List<ItemToReturnDto>();
    }
}
