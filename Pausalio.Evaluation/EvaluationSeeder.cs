using Microsoft.EntityFrameworkCore;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Persistence;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pausalio.Evaluation
{
    public class EvaluationSeeder
    {
        public static readonly Guid BusinessProfileId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        
        public static async Task SeedAsync(PausalioDbContext context, string apiKey, string modelName)
        {
            await context.Database.EnsureCreatedAsync();
            await CleanExistingDataAsync(context);
            await SeedCoreAsync(context, apiKey, modelName);
        }

        private static async Task CleanExistingDataAsync(PausalioDbContext context)
        {
            // Clean AI logs associated with evaluation conversations
            var conversationIds = await context.Set<AiConversation>()
                .Where(c => c.UserId == UserId || c.BusinessProfileId == BusinessProfileId)
                .Select(c => c.Id)
                .ToListAsync();

            if (conversationIds.Any())
            {
                var messageIds = await context.Set<AiMessage>()
                    .Where(m => conversationIds.Contains(m.ConversationId))
                    .Select(m => m.Id)
                    .ToListAsync();

                if (messageIds.Any())
                {
                    var toolCalls = await context.Set<AiToolCall>()
                        .Where(tc => messageIds.Contains(tc.MessageId))
                        .ToListAsync();
                    context.Set<AiToolCall>().RemoveRange(toolCalls);

                    var messages = await context.Set<AiMessage>()
                        .Where(m => messageIds.Contains(m.Id))
                        .ToListAsync();
                    context.Set<AiMessage>().RemoveRange(messages);
                }

                var conversations = await context.Set<AiConversation>()
                    .Where(c => conversationIds.Contains(c.Id))
                    .ToListAsync();
                context.Set<AiConversation>().RemoveRange(conversations);
            }

            // Clean core data
            var reminders = await context.Reminders.Where(r => r.BusinessProfileId == BusinessProfileId).ToListAsync();
            context.Reminders.RemoveRange(reminders);

            var taxObligations = await context.TaxObligations.Where(t => t.BusinessProfileId == BusinessProfileId).ToListAsync();
            context.TaxObligations.RemoveRange(taxObligations);

            var expenses = await context.Expenses.Where(e => e.BusinessProfileId == BusinessProfileId).ToListAsync();
            context.Expenses.RemoveRange(expenses);

            var payments = await context.Payments.Where(p => p.BusinessProfileId == BusinessProfileId).ToListAsync();
            context.Payments.RemoveRange(payments);

            var invoices = await context.Invoices.Where(i => i.BusinessProfileId == BusinessProfileId).ToListAsync();
            context.Invoices.RemoveRange(invoices);

            var bankAccounts = await context.BankAccounts.Where(b => b.BusinessProfileId == BusinessProfileId).ToListAsync();
            context.BankAccounts.RemoveRange(bankAccounts);

            var clients = await context.Clients.Where(c => c.BusinessProfileId == BusinessProfileId).ToListAsync();
            context.Clients.RemoveRange(clients);

            var userBusinessProfiles = await context.UserBusinessProfiles.Where(ubp => ubp.BusinessProfileId == BusinessProfileId).ToListAsync();
            context.UserBusinessProfiles.RemoveRange(userBusinessProfiles);

            var businessProfile = await context.BusinessProfiles.FindAsync(BusinessProfileId);
            if (businessProfile != null)
                context.BusinessProfiles.Remove(businessProfile);

            var userProfile = await context.UserProfiles.FindAsync(UserId);
            if (userProfile != null)
                context.UserProfiles.Remove(userProfile);

            await context.SaveChangesAsync();
        }

        private static async Task SeedCoreAsync(PausalioDbContext context, string apiKey, string modelName)
        {
            // 1. Activity Code
            var activityCodeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var activityCode = await context.ActivityCodes.FindAsync(activityCodeId);
            if (activityCode == null)
            {
                activityCode = new ActivityCode
                {
                    Id = activityCodeId,
                    Code = "6201",
                    Description = "Računarsko programiranje"
                };
                await context.ActivityCodes.AddAsync(activityCode);
            }

            // 2. User Profile
            var user = new UserProfile
            {
                Id = UserId,
                FirstName = "Eval",
                LastName = "User",
                Email = "eval@pausalio.rs",
                PasswordHash = "dummyhash_not_used_in_evals",
                City = "Beograd",
                Address = "Knez Mihailova 1",
                Phone = "+38111123456",
                Role = UserRole.RegularUser,
                IsActive = true,
                IsEmailVerified = true,
                OpenRouterApiKey = apiKey,
                OpenRouterModelName = modelName,
                CreatedAt = DateTime.UtcNow
            };
            await context.UserProfiles.AddAsync(user);

            // 3. Business Profile
            var business = new BusinessProfile
            {
                Id = BusinessProfileId,
                BusinessName = "Eval DOO",
                PIB = "123456789",
                MB = "98765432",
                ActivityCodeId = activityCode.Id,
                City = "Beograd",
                Address = "Knez Mihailova 1",
                Email = "eval@pausalio.rs",
                Phone = "+38111123456",
                Website = "https://eval.pausalio.rs",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.BusinessProfiles.AddAsync(business);

            // 4. User Business Profile LINK
            var link = new UserBusinessProfile
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                BusinessProfileId = BusinessProfileId,
                Role = UserBusinessRole.Owner,
                CreatedAt = DateTime.UtcNow
            };
            await context.UserBusinessProfiles.AddAsync(link);

            // 5. Countries (Ensured)
            var countries = new List<Country>();
            var serbia = await context.Countries.FirstOrDefaultAsync(c => c.Code == "RS");
            if (serbia == null)
            {
                serbia = new Country { Id = Guid.NewGuid(), Name = "Srbija", Code = "RS" };
                await context.Countries.AddAsync(serbia);
            }
            countries.Add(serbia);

            var germany = await context.Countries.FirstOrDefaultAsync(c => c.Code == "DE");
            if (germany == null)
            {
                germany = new Country { Id = Guid.NewGuid(), Name = "Nemačka", Code = "DE" };
                await context.Countries.AddAsync(germany);
            }
            countries.Add(germany);

            var usa = await context.Countries.FirstOrDefaultAsync(c => c.Code == "US");
            if (usa == null)
            {
                usa = new Country { Id = Guid.NewGuid(), Name = "SAD", Code = "US" };
                await context.Countries.AddAsync(usa);
            }
            countries.Add(usa);

            // 6. Clients
            var clientA = new Client
            {
                Id = Guid.Parse("33333333-3333-3333-3333-bcd111111111"),
                BusinessProfileId = BusinessProfileId,
                ClientType = ClientType.Domestic,
                Name = "TechCorp d.o.o.",
                PIB = "101111111",
                MB = "17111111",
                Address = "Milutina Milankovića 9",
                City = "Novi Beograd",
                Email = "billing@techcorp.rs",
                Phone = "+38111222333",
                CountryId = serbia.Id,
                IsActive = true,
                CreatedAt = new DateTime(2023, 12, 1)
            };
            var clientB = new Client
            {
                Id = Guid.Parse("33333333-3333-3333-3333-bcd222222222"),
                BusinessProfileId = BusinessProfileId,
                ClientType = ClientType.Domestic,
                Name = "GlobalSoft SRB",
                PIB = "102222222",
                MB = "17222222",
                Address = "Bulevar Oslobođenja 44",
                City = "Novi Sad",
                Email = "finance@globalsoft.rs",
                Phone = "+38121333444",
                CountryId = serbia.Id,
                IsActive = true,
                CreatedAt = new DateTime(2023, 12, 5)
            };
            var clientC = new Client
            {
                Id = Guid.Parse("33333333-3333-3333-3333-bcd333333333"),
                BusinessProfileId = BusinessProfileId,
                ClientType = ClientType.Foreign,
                Name = "München Consulting GmbH",
                PIB = "DE987654321",
                Address = "Leopoldstraße 12",
                City = "München",
                Email = "accounts@muenchenconsulting.de",
                CountryId = germany.Id,
                IsActive = true,
                CreatedAt = new DateTime(2023, 12, 10)
            };
            var clientD = new Client
            {
                Id = Guid.Parse("33333333-3333-3333-3333-bcd444444444"),
                BusinessProfileId = BusinessProfileId,
                ClientType = ClientType.Individual,
                Name = "Marko Marković PR",
                PIB = "104444444",
                MB = "20444444",
                Address = "Vojvode Mišića 12",
                City = "Niš",
                Email = "marko@pr.rs",
                CountryId = serbia.Id,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 15)
            };
            var clientE = new Client
            {
                Id = Guid.Parse("33333333-3333-3333-3333-bcd555555555"),
                BusinessProfileId = BusinessProfileId,
                ClientType = ClientType.Domestic,
                Name = "Legacy d.o.o. (Neaktivan)",
                PIB = "105555555",
                Address = "Kralja Aleksandra 20",
                City = "Kragujevac",
                Email = "info@legacy.rs",
                CountryId = serbia.Id,
                IsActive = false,
                CreatedAt = new DateTime(2023, 1, 1)
            };

            await context.Clients.AddRangeAsync(clientA, clientB, clientC, clientD, clientE);

            // 7. Bank Accounts
            var accountRsd = new BankAccount
            {
                Id = Guid.NewGuid(),
                BusinessProfileId = BusinessProfileId,
                BankName = "Banca Intesa AD",
                AccountNumber = "160-123456789-01",
                Currency = Currency.RSD,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var accountEur = new BankAccount
            {
                Id = Guid.NewGuid(),
                BusinessProfileId = BusinessProfileId,
                BankName = "OTP Banka Srbija",
                AccountNumber = "325-987654321-99",
                Currency = Currency.EUR,
                IBAN = "RS35325000009876543219",
                SWIFT = "OTPVRS22",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.BankAccounts.AddRangeAsync(accountRsd, accountEur);

            // 8. Invoices & Payments & InvoiceItems
            var invoices = new List<Invoice>();
            var payments = new List<Payment>();

            // Let's seed 40 deterministic invoices across 2024 and 2025.
            // 20 Invoices in 2024, 20 Invoices in 2025
            
            // --- YEAR 2024 ---
            for (int i = 1; i <= 20; i++)
            {
                var issueDate = new DateTime(2024, (i % 12) + 1, (i * 2) % 28 + 1);
                var dueDate = issueDate.AddDays(15);
                var isForeign = (i % 3 == 0); // Alternate domestic/foreign
                var client = isForeign ? clientC : (i % 2 == 0 ? clientA : clientB);
                var currency = isForeign ? Currency.EUR : Currency.RSD;
                var exchangeRate = isForeign ? 117.2m : 1.0m;
                var totalAmount = isForeign ? 1000m + (i * 100) : 50000m + (i * 5000);
                var totalAmountRsd = totalAmount * exchangeRate;

                // Alternate invoice status and payment status
                InvoiceStatus invStatus;
                PaymentStatus payStatus;
                
                if (i % 5 == 1) // Paid
                {
                    invStatus = InvoiceStatus.Finished;
                    payStatus = PaymentStatus.Paid;
                }
                else if (i % 5 == 2) // Unpaid (Overdue now since it's 2024)
                {
                    invStatus = InvoiceStatus.Sent;
                    payStatus = PaymentStatus.Unpaid;
                }
                else if (i % 5 == 3) // Partially Paid
                {
                    invStatus = InvoiceStatus.Sent;
                    payStatus = PaymentStatus.PartiallyPaid;
                }
                else if (i % 5 == 4) // Cancelled
                {
                    invStatus = InvoiceStatus.Cancelled;
                    payStatus = PaymentStatus.Unpaid;
                }
                else // Archived/Finished & Paid
                {
                    invStatus = InvoiceStatus.Archived;
                    payStatus = PaymentStatus.Paid;
                }

                var invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    BusinessProfileId = BusinessProfileId,
                    ClientId = client.Id,
                    InvoiceNumber = $"F-2024-{i:D3}",
                    InvoiceStatus = invStatus,
                    PaymentStatus = payStatus,
                    Currency = currency,
                    ExchangeRate = exchangeRate,
                    TotalAmount = totalAmount,
                    TotalAmountRSD = totalAmountRsd,
                    AmountToPay = payStatus == PaymentStatus.Paid ? 0 : (payStatus == PaymentStatus.PartiallyPaid ? totalAmount / 2 : totalAmount),
                    IssueDate = issueDate,
                    DueDate = dueDate,
                    ReferenceNumber = $"97-12345{i:D2}",
                    Notes = $"Hvala na saradnji za period {issueDate:MM/yyyy}.",
                    CreatedAt = issueDate.AddHours(2)
                };

                // Add Items
                invoice.Items.Add(new InvoiceItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    Name = i % 2 == 0 ? "Razvoj softvera" : "Savetovanje i podrška",
                    ItemType = i % 2 == 0 ? ItemType.Service : ItemType.Product,
                    Quantity = 1,
                    UnitPrice = totalAmount,
                    TotalPrice = totalAmount
                });

                invoices.Add(invoice);

                // Add Payments for Paid/PartiallyPaid
                if (payStatus == PaymentStatus.Paid)
                {
                    payments.Add(new Payment
                    {
                        Id = Guid.NewGuid(),
                        BusinessProfileId = BusinessProfileId,
                        InvoiceId = invoice.Id,
                        PaymentType = PaymentType.InvoicePayment,
                        Amount = totalAmount,
                        Currency = currency,
                        ExchangeRate = exchangeRate,
                        AmountRSD = totalAmountRsd,
                        PaymentDate = dueDate.AddDays(-2), // Paid code, slightly early
                        ReferenceNumber = $"B-2024-P{i:D3}",
                        Description = $"Uplata po fakturi {invoice.InvoiceNumber}",
                        CreatedAt = dueDate.AddDays(-2).AddHours(9),
                        BankAccountId = currency == Currency.RSD ? accountRsd.Id : accountEur.Id
                    });
                }
                else if (payStatus == PaymentStatus.PartiallyPaid)
                {
                    payments.Add(new Payment
                    {
                        Id = Guid.NewGuid(),
                        BusinessProfileId = BusinessProfileId,
                        InvoiceId = invoice.Id,
                        PaymentType = PaymentType.InvoicePayment,
                        Amount = totalAmount / 2,
                        Currency = currency,
                        ExchangeRate = exchangeRate,
                        AmountRSD = totalAmountRsd / 2,
                        PaymentDate = dueDate.AddDays(2), // Paid code, slightly late
                        ReferenceNumber = $"B-2024-P{i:D3}-1",
                        Description = $"Parcijalna uplata po fakturi {invoice.InvoiceNumber}",
                        CreatedAt = dueDate.AddDays(2).AddHours(9),
                        BankAccountId = currency == Currency.RSD ? accountRsd.Id : accountEur.Id
                    });
                }
            }

            // --- YEAR 2025 ---
            for (int i = 1; i <= 20; i++)
            {
                var issueDate = new DateTime(2025, (i % 12) + 1, (i * 2) % 28 + 1);
                var dueDate = issueDate.AddDays(15);
                var isForeign = (i % 4 == 0); // Alternate domestic/foreign
                var client = isForeign ? clientC : (i % 3 == 0 ? clientB : (i % 2 == 0 ? clientA : clientD));
                var currency = isForeign ? Currency.EUR : Currency.RSD;
                var exchangeRate = isForeign ? 117.0m : 1.0m;
                var totalAmount = isForeign ? 1200m + (i * 100) : 40000m + (i * 6000);
                var totalAmountRsd = totalAmount * exchangeRate;

                // Alternate invoice status and payment status
                InvoiceStatus invStatus;
                PaymentStatus payStatus;

                // Since local time is 2026, all 2025 invoices are in the past.
                if (i % 4 == 1) // Paid
                {
                    invStatus = InvoiceStatus.Finished;
                    payStatus = PaymentStatus.Paid;
                }
                else if (i % 4 == 2) // Unpaid (Overdue now since it's 2025)
                {
                    invStatus = InvoiceStatus.Sent;
                    payStatus = PaymentStatus.Unpaid;
                }
                else if (i % 4 == 3) // Partially Paid
                {
                    invStatus = InvoiceStatus.Sent;
                    payStatus = PaymentStatus.PartiallyPaid;
                }
                else // Cancelled
                {
                    invStatus = InvoiceStatus.Cancelled;
                    payStatus = PaymentStatus.Unpaid;
                }

                var invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    BusinessProfileId = BusinessProfileId,
                    ClientId = client.Id,
                    InvoiceNumber = $"F-2025-{i:D3}",
                    InvoiceStatus = invStatus,
                    PaymentStatus = payStatus,
                    Currency = currency,
                    ExchangeRate = exchangeRate,
                    TotalAmount = totalAmount,
                    TotalAmountRSD = totalAmountRsd,
                    AmountToPay = payStatus == PaymentStatus.Paid ? 0 : (payStatus == PaymentStatus.PartiallyPaid ? totalAmount / 2 : totalAmount),
                    IssueDate = issueDate,
                    DueDate = dueDate,
                    ReferenceNumber = $"97-23456{i:D2}",
                    Notes = $"Konsultantske usluge za {issueDate:MMMM yyyy}.",
                    CreatedAt = issueDate.AddHours(2)
                };

                // Add Items
                invoice.Items.Add(new InvoiceItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    Name = i % 2 == 0 ? "Razvoj softvera i integracija" : "IT Konsultacije",
                    ItemType = ItemType.Service,
                    Quantity = i % 3 + 1,
                    UnitPrice = totalAmount / (i % 3 + 1),
                    TotalPrice = totalAmount
                });

                invoices.Add(invoice);

                // Add Payments for Paid/Partially Paid
                if (payStatus == PaymentStatus.Paid)
                {
                    payments.Add(new Payment
                    {
                        Id = Guid.NewGuid(),
                        BusinessProfileId = BusinessProfileId,
                        InvoiceId = invoice.Id,
                        PaymentType = PaymentType.InvoicePayment,
                        Amount = totalAmount,
                        Currency = currency,
                        ExchangeRate = exchangeRate,
                        AmountRSD = totalAmountRsd,
                        PaymentDate = dueDate.AddDays(-1),
                        ReferenceNumber = $"B-2025-P{i:D3}",
                        Description = $"Uplata po fakturi {invoice.InvoiceNumber}",
                        CreatedAt = dueDate.AddDays(-1).AddHours(10),
                        BankAccountId = currency == Currency.RSD ? accountRsd.Id : accountEur.Id
                    });
                }
                else if (payStatus == PaymentStatus.PartiallyPaid)
                {
                    payments.Add(new Payment
                    {
                        Id = Guid.NewGuid(),
                        BusinessProfileId = BusinessProfileId,
                        InvoiceId = invoice.Id,
                        PaymentType = PaymentType.InvoicePayment,
                        Amount = totalAmount / 2,
                        Currency = currency,
                        ExchangeRate = exchangeRate,
                        AmountRSD = totalAmountRsd / 2,
                        PaymentDate = dueDate.AddDays(5), // Paid late
                        ReferenceNumber = $"B-2025-P{i:D3}-1",
                        Description = $"Parcijalna uplata po fakturi {invoice.InvoiceNumber}",
                        CreatedAt = dueDate.AddDays(5).AddHours(10),
                        BankAccountId = currency == Currency.RSD ? accountRsd.Id : accountEur.Id
                    });
                }
            }

            await context.Invoices.AddRangeAsync(invoices);
            await context.Payments.AddRangeAsync(payments);

            // 9. Expenses (12 expenses)
            var expenses = new List<Expense>();
            var expenseTypes = new[] { "Zakup kancelarije", "Internet i telefon", "Adobe Creative Cloud", "Knjigovodstvene usluge", "Cloud Hosting", "Kancelarijski nameštaj" };
            
            for (int i = 1; i <= 12; i++)
            {
                var year = i <= 6 ? 2024 : 2025;
                var createdDate = new DateTime(year, (i * 2 - 1) % 12 + 1, 10);
                var amount = i * 4000m + 5000m;
                var status = i % 3 == 0 ? ExpenseStatus.Pending : (i % 3 == 1 ? ExpenseStatus.Paid : ExpenseStatus.Archived);

                expenses.Add(new Expense
                {
                    Id = Guid.NewGuid(),
                    BusinessProfileId = BusinessProfileId,
                    Name = expenseTypes[(i - 1) % expenseTypes.Length],
                    Amount = amount,
                    Status = status,
                    ReferenceNumber = $"EXP-{year}-{i:D3}",
                    CreatedAt = createdDate
                });
            }
            await context.Expenses.AddRangeAsync(expenses);

            // 10. Tax Obligations (PIO, Health, Unemployment for 2024 and 2025)
            var taxObligations = new List<TaxObligation>();
            
            for (int y = 2024; y <= 2025; y++)
            {
                var monthlyPIO = y == 2024 ? 14800m : 15500m;
                var monthlyHealth = y == 2024 ? 5400m : 5800m;

                for (int m = 1; m <= 12; m++)
                {
                    var dueDate = new DateTime(y, m, 15).AddMonths(1); // Due 15th of next month
                    
                    // PIO Taxes
                    var pioStatus = (y == 2025 && m >= 11) ? TaxObligationStatus.Pending : TaxObligationStatus.Paid;
                    var pioPaidDate = pioStatus == TaxObligationStatus.Paid ? (DateTime?)dueDate.AddDays((m % 3) - 1) : null; // Some slightly late/early
                    
                    taxObligations.Add(new TaxObligation
                    {
                        Id = Guid.NewGuid(),
                        BusinessProfileId = BusinessProfileId,
                        Year = y,
                        Month = m,
                        Type = TaxObligationType.PIO,
                        TotalAmount = monthlyPIO,
                        Status = pioStatus,
                        DueDate = dueDate,
                        PaidDate = pioPaidDate,
                        ReferenceNumber = $"REF-PIO-{y}-{m:D2}",
                        CreatedAt = new DateTime(y, m, 1)
                    });

                    // Health Taxes (Quarterly in our setup to mix it up)
                    if (m % 3 == 0)
                    {
                        var healthStatus = (y == 2025 && m == 12) ? TaxObligationStatus.Pending : TaxObligationStatus.Paid;
                        var healthPaidDate = healthStatus == TaxObligationStatus.Paid ? (DateTime?)dueDate.AddDays(m % 2 == 0 ? 3 : -2) : null; // mix of late delays
                        
                        taxObligations.Add(new TaxObligation
                        {
                            Id = Guid.NewGuid(),
                            BusinessProfileId = BusinessProfileId,
                            Year = y,
                            Month = m,
                            Type = TaxObligationType.Health,
                            TotalAmount = monthlyHealth * 3,
                            Status = healthStatus,
                            DueDate = dueDate,
                            PaidDate = healthPaidDate,
                            ReferenceNumber = $"REF-HLT-{y}-{m:D2}",
                            CreatedAt = new DateTime(y, m, 1)
                        });
                    }
                }
            }
            await context.TaxObligations.AddRangeAsync(taxObligations);

            // 11. Reminders (5)
            var reminders = new List<Reminder>
            {
                new Reminder
                {
                    Id = Guid.NewGuid(),
                    BusinessProfileId = BusinessProfileId,
                    Title = "Platiti doprinose za decembar 2025",
                    Description = "Rok je 15. januar 2026. godine.",
                    ReminderType = ReminderType.Tax,
                    DueDate = new DateTime(2026, 1, 15),
                    IsCompleted = false,
                    CreatedAt = new DateTime(2025, 12, 31)
                },
                new Reminder
                {
                    Id = Guid.NewGuid(),
                    BusinessProfileId = BusinessProfileId,
                    Title = "Predaja M4 obrasca",
                    Description = "Konsultacija sa knjigovođom.",
                    ReminderType = ReminderType.Other,
                    DueDate = new DateTime(2026, 4, 30),
                    IsCompleted = false,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Reminder
                {
                    Id = Guid.NewGuid(),
                    BusinessProfileId = BusinessProfileId,
                    Title = "Obnova domena pausalio.rs",
                    Description = "Uplatiti naknadu za registraciju domena.",
                    ReminderType = ReminderType.Expense,
                    DueDate = new DateTime(2025, 11, 20),
                    IsCompleted = true,
                    CompletedAt = new DateTime(2025, 11, 18),
                    CreatedAt = new DateTime(2025, 10, 20)
                },
                new Reminder
                {
                    Id = Guid.NewGuid(),
                    BusinessProfileId = BusinessProfileId,
                    Title = "Sastanak sa klijentom TechCorp",
                    Description = "Dogovor o novom ugovoru za 2026. godinu.",
                    ReminderType = ReminderType.Meeting,
                    DueDate = new DateTime(2026, 1, 10),
                    IsCompleted = true,
                    CompletedAt = new DateTime(2026, 1, 10),
                    CreatedAt = new DateTime(2026, 1, 5)
                },
                new Reminder
                {
                    Id = Guid.NewGuid(),
                    BusinessProfileId = BusinessProfileId,
                    Title = "Fakturisati decembar",
                    Description = "Poslati fakture svim mesečnim klijentima.",
                    ReminderType = ReminderType.Expense,
                    DueDate = new DateTime(2025, 12, 31),
                    IsCompleted = true,
                    CompletedAt = new DateTime(2025, 12, 28),
                    CreatedAt = new DateTime(2025, 12, 15)
                }
            };
            await context.Reminders.AddRangeAsync(reminders);

            await context.SaveChangesAsync();
        }
    }
}
