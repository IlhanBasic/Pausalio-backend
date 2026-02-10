using Microsoft.EntityFrameworkCore;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Infrastructure.Persistence
{
    public class PausalioDbContext: DbContext
    {
        public PausalioDbContext(DbContextOptions<PausalioDbContext> options) : base(options) { }
        public DbSet<BusinessProfile> BusinessProfiles { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserBusinessProfile> UserBusinessProfiles { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoicesItem { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<TaxObligation> TaxObligations { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<ActivityCode> ActivityCodes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PausalioDbContext).Assembly);
        }
    }
}
