using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Domain.Entities
{
    public class BusinessProfile
    {
        public Guid Id { get; set; }
        public string BusinessName { get; set; } = null!;
        public string PIB { get; set; } = null!;
        public string MB { get; set; } = string.Empty;
        public Guid ActivityCodeId { get; set; }
        public ActivityCode ActivityCode { get; set; } = null!;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? CompanyLogo { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<UserBusinessProfile> UserBusinessProfiles { get; set; } = new List<UserBusinessProfile>();
        public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
        public ICollection<Client> Clients { get; set; } = new List<Client>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public ICollection<TaxObligation> TaxObligations { get; set; } = new List<TaxObligation>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<BusinessInvite> BusinessInvites { get; set; } = new List<BusinessInvite>();

    }
}
