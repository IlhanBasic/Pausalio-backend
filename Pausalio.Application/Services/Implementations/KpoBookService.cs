using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pausalio.Application.DTOs.KpoBook;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Constants;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Pausalio.Application.Services.Implementations
{
    public class KpoBookService : IKpoBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPdfFactoryService _pdfFactoryService;

        public KpoBookService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IPdfFactoryService pdfFactoryService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _pdfFactoryService = pdfFactoryService;
        }

        // ──────────────── Kreiranje KPO zapisa ────────────────

        public async Task<KpoEntryToReturnDto> CreateEntryForInvoiceAsync(Guid invoiceId)
        {
            var companyId = GetCompanyIdOrThrow();

            var invoice = await _unitOfWork.InvoiceRepository
                .FindInvoiceWithEntities(x => x.Id == invoiceId &&
                                              x.BusinessProfileId == companyId &&
                                              !x.IsDeleted);

            if (invoice == null)
                throw new KeyNotFoundException("Faktura nije pronađena.");

            if (invoice.InvoiceStatus == InvoiceStatus.Draft)
                throw new InvalidOperationException("KPO zapis se ne može kreirati za draft fakturu.");

            // Proveri da li već postoji regularni KPO zapis za ovu fakturu
            var existing = await _unitOfWork.KpoEntryRepository
                .FindRegularByInvoiceIdAsync(companyId, invoiceId);

            if (existing != null)
                throw new InvalidOperationException("KPO zapis već postoji za ovu fakturu.");

            var entry = await BuildKpoEntryFromInvoice(invoice, companyId, KpoEntryType.Regular);

            await _unitOfWork.KpoEntryRepository.AddAsync(entry);
            await _unitOfWork.SaveChangesAsync();

            // Proveri threshold-e i kreiraj upozorenja
            await CheckAndCreateThresholdRemindersAsync(companyId, entry.Year);

            // Vrati sa invoice numberom
            var created = await _unitOfWork.KpoEntryRepository.GetByIdAsync(entry.Id);
            return _mapper.Map<KpoEntryToReturnDto>(entry);
        }

        public async Task<KpoEntryToReturnDto> CreateStornoEntryAsync(Guid invoiceId)
        {
            var companyId = GetCompanyIdOrThrow();

            var originalEntry = await _unitOfWork.KpoEntryRepository
                .FindRegularByInvoiceIdAsync(companyId, invoiceId);

            if (originalEntry == null)
                return null!; // Faktura nema KPO zapis, nema šta da se stornira

            var invoice = await _unitOfWork.InvoiceRepository
                .FindInvoiceWithEntities(x => x.Id == invoiceId &&
                                              x.BusinessProfileId == companyId);

            if (invoice == null)
                throw new KeyNotFoundException("Faktura nije pronađena.");

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var nextSeq = await _unitOfWork.KpoEntryRepository
                    .GetNextSequenceNumberAsync(companyId, originalEntry.Year);

                var stornoEntry = new KpoEntry
                {
                    Id = Guid.NewGuid(),
                    BusinessProfileId = companyId,
                    Year = originalEntry.Year,
                    SequenceNumber = nextSeq,
                    EntryDate = DateTime.UtcNow,
                    Description = $"STORNO - Faktura {invoice.InvoiceNumber}",
                    ProductIncome = -originalEntry.ProductIncome,
                    ServiceIncome = -originalEntry.ServiceIncome,
                    TotalIncome = -originalEntry.TotalIncome,
                    OriginalCurrency = originalEntry.OriginalCurrency,
                    OriginalAmount = -originalEntry.OriginalAmount,
                    ExchangeRate = originalEntry.ExchangeRate,
                    EntryType = KpoEntryType.Storno,
                    InvoiceId = invoiceId,
                    OriginalEntryId = originalEntry.Id,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.KpoEntryRepository.AddAsync(stornoEntry);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<KpoEntryToReturnDto>(stornoEntry);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ──────────────── Čitanje ────────────────

        public async Task<IEnumerable<KpoEntryToReturnDto>> GetEntriesByYearAsync(int year)
        {
            var companyId = GetCompanyIdOrThrow();

            var entries = await _unitOfWork.KpoEntryRepository.GetByYearAsync(companyId, year);
            return _mapper.Map<IEnumerable<KpoEntryToReturnDto>>(entries);
        }

        public async Task<KpoEntryToReturnDto?> GetByIdAsync(Guid id)
        {
            var companyId = GetCompanyIdOrThrow();

            var entry = await _unitOfWork.KpoEntryRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId);

            if (entry == null)
                return null;

            return _mapper.Map<KpoEntryToReturnDto>(entry);
        }

        // ──────────────── Godišnji sumar ────────────────

        public async Task<KpoYearlySummaryDto> GetYearlySummaryAsync(int year)
        {
            var companyId = GetCompanyIdOrThrow();

            var entries = await _unitOfWork.KpoEntryRepository.GetByYearAsync(companyId, year);

            var totalProduct = entries.Sum(x => x.ProductIncome);
            var totalService = entries.Sum(x => x.ServiceIncome);
            var grandTotal = entries.Sum(x => x.TotalIncome);

            // VAT exposure: isključi usluge fakturisane stranim klijentima
            // (mesto prometa u inostranstvu po osnovu B2B pravila)
            var invoiceIds = entries.Select(x => x.InvoiceId).Distinct().ToList();
            var invoices = await _unitOfWork.InvoiceRepository
                .FindInvoicesWithEntities(x => invoiceIds.Contains(x.Id));

            var foreignInvoiceIds = invoices
                .Where(i => i.Client != null && i.Client.ClientType == ClientType.Foreign)
                .Select(i => i.Id)
                .ToHashSet();

            // Za VAT: ukupno - usluge za strane klijente
            var foreignServiceIncome = entries
                .Where(x => foreignInvoiceIds.Contains(x.InvoiceId))
                .Sum(x => x.ServiceIncome);

            var vatExposure = grandTotal - foreignServiceIncome;

            var alerts = BuildAlerts(grandTotal);

            return new KpoYearlySummaryDto
            {
                Year = year,
                TotalProductIncome = totalProduct,
                TotalServiceIncome = totalService,
                GrandTotal = grandTotal,
                EntryCount = entries.Count,
                RemainingTo6MLimit = KpoConstants.LimitSelfEmployed - grandTotal,
                RemainingTo8MLimit = KpoConstants.LimitVatRegistration - vatExposure,
                VatExposureTotal = vatExposure,
                Alerts = alerts
            };
        }

        // ──────────────── PDF Eksport ────────────────

        public async Task<byte[]> ExportPdfAsync(int year)
        {
            var companyId = GetCompanyIdOrThrow();

            var entries = await _unitOfWork.KpoEntryRepository.GetByYearAsync(companyId, year);
            var business = await _unitOfWork.BusinessProfileRepository.GetByIdAsync(companyId);

            if (business == null)
                throw new KeyNotFoundException("Poslovni profil nije pronađen.");

            return _pdfFactoryService.GeneratePdf(() => BuildKpoPdfDocument(entries, business, year));
        }

        // ──────────────── Backfill ────────────────

        public async Task<int> BackfillFromExistingInvoicesAsync(int year)
        {
            var companyId = GetCompanyIdOrThrow();

            // Nađi sve fakture za godinu koje NISU draft/cancelled/deleted i NEMAJU KPO zapis
            var invoices = await _unitOfWork.InvoiceRepository
                .FindInvoicesWithEntities(x => x.BusinessProfileId == companyId &&
                                                x.IssueDate.Year == year &&
                                                !x.IsDeleted &&
                                                x.InvoiceStatus != InvoiceStatus.Draft &&
                                                x.InvoiceStatus != InvoiceStatus.Cancelled);

            // Sortiraj hronološki
            var sortedInvoices = invoices.OrderBy(x => x.IssueDate).ThenBy(x => x.CreatedAt).ToList();

            int count = 0;

            foreach (var invoice in sortedInvoices)
            {
                // Proveri da li već postoji KPO zapis
                var existing = await _unitOfWork.KpoEntryRepository
                    .FindRegularByInvoiceIdAsync(companyId, invoice.Id);

                if (existing != null)
                    continue;

                var entry = await BuildKpoEntryFromInvoice(invoice, companyId, KpoEntryType.Regular);
                await _unitOfWork.KpoEntryRepository.AddAsync(entry);
                await _unitOfWork.SaveChangesAsync();
                count++;
            }

            // Proveri threshold-e na kraju
            await CheckAndCreateThresholdRemindersAsync(companyId, year);

            return count;
        }

        // ──────────────── Privatne metode ────────────────

        private async Task<KpoEntry> BuildKpoEntryFromInvoice(Invoice invoice, Guid companyId, KpoEntryType entryType)
        {
            int year = invoice.IssueDate.Year;
            var nextSeq = await _unitOfWork.KpoEntryRepository
                .GetNextSequenceNumberAsync(companyId, year);

            // Razdeli ukupan RSD iznos po tipu stavki
            decimal productIncome = 0;
            decimal serviceIncome = 0;

            if (invoice.Items != null && invoice.Items.Any())
            {
                var totalOriginal = invoice.Items.Sum(x => x.TotalPrice);
                if (totalOriginal > 0)
                {
                    foreach (var item in invoice.Items)
                    {
                        // Proporcionalni RSD iznos po stavci
                        var proportion = item.TotalPrice / totalOriginal;
                        var rsdAmount = invoice.TotalAmountRSD * proportion;

                        if (item.ItemType == ItemType.Product)
                            productIncome += rsdAmount;
                        else
                            serviceIncome += rsdAmount;
                    }
                }
            }
            else
            {
                // Ako nema stavki (ne bi trebalo), stavi sve u usluge
                serviceIncome = invoice.TotalAmountRSD;
            }

            // Zaokruži na 2 decimale
            productIncome = Math.Round(productIncome, 2);
            serviceIncome = Math.Round(serviceIncome, 2);
            var totalIncome = productIncome + serviceIncome;

            // Opis: "Faktura {broj} - {klijent}"
            var clientName = invoice.Client?.Name ?? "";
            var description = string.IsNullOrEmpty(clientName)
                ? $"Faktura {invoice.InvoiceNumber}"
                : $"Faktura {invoice.InvoiceNumber} - {clientName}";

            return new KpoEntry
            {
                Id = Guid.NewGuid(),
                BusinessProfileId = companyId,
                Year = year,
                SequenceNumber = nextSeq,
                EntryDate = invoice.IssueDate,
                Description = description,
                ProductIncome = productIncome,
                ServiceIncome = serviceIncome,
                TotalIncome = totalIncome,
                OriginalCurrency = invoice.Currency,
                OriginalAmount = invoice.TotalAmount,
                ExchangeRate = invoice.ExchangeRate,
                EntryType = entryType,
                InvoiceId = invoice.Id,
                CreatedAt = DateTime.UtcNow
            };
        }

        private async Task CheckAndCreateThresholdRemindersAsync(Guid companyId, int year)
        {
            var entries = await _unitOfWork.KpoEntryRepository.GetByYearAsync(companyId, year);
            var grandTotal = entries.Sum(x => x.TotalIncome);

            var thresholds = new[]
            {
                (Name: "Promet 4M RSD", Amount: KpoConstants.Threshold4M),
                (Name: "Promet 5M RSD", Amount: KpoConstants.Threshold5M),
                (Name: "Paušalni limit 6M RSD", Amount: KpoConstants.LimitSelfEmployed),
            };

            foreach (var threshold in thresholds)
            {
                if (grandTotal >= threshold.Amount)
                {
                    // Proveri da li reminder za ovaj prag i godinu već postoji
                    var reminderTitle = $"KPO upozorenje: {threshold.Name} ({year})";
                    var existingReminder = await _unitOfWork.ReminderRepository
                        .FindFirstOrDefaultAsync(x => x.BusinessProfileId == companyId &&
                                                       x.Title == reminderTitle &&
                                                       x.ReminderType == ReminderType.KpoThreshold);

                    if (existingReminder == null)
                    {
                        var reminder = new Reminder
                        {
                            Id = Guid.NewGuid(),
                            BusinessProfileId = companyId,
                            Title = reminderTitle,
                            Description = $"Ukupan promet u KPO knjizi za {year}. godinu je dostigao {grandTotal:N0} RSD. Prag: {threshold.Amount:N0} RSD.",
                            ReminderType = ReminderType.KpoThreshold,
                            DueDate = DateTime.UtcNow,
                            IsCompleted = false,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.ReminderRepository.AddAsync(reminder);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
            }
        }

        private List<KpoAlertDto> BuildAlerts(decimal grandTotal)
        {
            var alerts = new List<KpoAlertDto>();
            var thresholds = new[]
            {
                ("Upozorenje 4M", KpoConstants.Threshold4M),
                ("Upozorenje 5M", KpoConstants.Threshold5M),
                ("Paušalni limit 6M", KpoConstants.LimitSelfEmployed),
                ("PDV prag 8M", KpoConstants.LimitVatRegistration),
            };

            foreach (var (name, amount) in thresholds)
            {
                alerts.Add(new KpoAlertDto
                {
                    ThresholdName = name,
                    ThresholdAmount = amount,
                    CurrentTotal = grandTotal,
                    IsExceeded = grandTotal >= amount
                });
            }

            return alerts;
        }

        private QuestPDF.Fluent.Document BuildKpoPdfDocument(
            IReadOnlyList<KpoEntry> entries, BusinessProfile business, int year)
        {
            return QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("KNJIGA O OSTVARENOM PROMETU")
                            .FontSize(14).Bold();
                        col.Item().AlignCenter().Text("PAUŠALNO OPOREZOVANIH OBVEZNIKA (KPO)")
                            .FontSize(12).Bold();
                        col.Item().Height(10);
                        col.Item().Text($"Naziv: {business.BusinessName}").FontSize(10);
                        col.Item().Text($"PIB: {business.PIB}   |   MB: {business.MB}").FontSize(10);
                        col.Item().Text($"Adresa: {business.Address}, {business.City}").FontSize(10);
                        col.Item().Text($"Godina: {year}").FontSize(10).Bold();
                        col.Item().Height(10);
                    });

                    // Content — tabela
                    page.Content().Table(table =>
                    {
                        // Definicija kolona
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);   // (1) R.br
                            columns.RelativeColumn(4);    // (2) Datum i opis
                            columns.RelativeColumn(2);    // (3) Prihod od proizvoda
                            columns.RelativeColumn(2);    // (4) Prihod od usluga
                            columns.RelativeColumn(2);    // (5) Svega
                        });

                        // Zaglavlje tabele
                        table.Header(header =>
                        {
                            header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .AlignCenter().Text("R.br\n(1)").Bold().FontSize(8);
                            header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .AlignCenter().Text("Datum i opis knjiženja\n(2)").Bold().FontSize(8);
                            header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .AlignCenter().Text("Prihod od prodaje\nproizvoda (3)").Bold().FontSize(8);
                            header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .AlignCenter().Text("Prihod od izvršenih\nusluga (4)").Bold().FontSize(8);
                            header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .AlignCenter().Text("Svega\n(5)").Bold().FontSize(8);
                        });

                        // Redovi
                        foreach (var entry in entries)
                        {
                            var bgColor = entry.EntryType == KpoEntryType.Storno
                                ? Colors.Red.Lighten5 : Colors.White;

                            // (1) Redni broj
                            table.Cell().Border(1).Background(bgColor).Padding(4)
                                .AlignCenter().Text(entry.SequenceNumber.ToString());

                            // (2) Datum i opis
                            table.Cell().Border(1).Background(bgColor).Padding(4).Column(descCol =>
                            {
                                descCol.Item().Text(entry.EntryDate.ToString("dd.MM.yyyy"));
                                descCol.Item().Text(entry.Description).FontSize(8);

                                // Za devizne fakture — prikaži originalni iznos i kurs
                                if (entry.OriginalCurrency != Currency.RSD)
                                {
                                    descCol.Item().Text(
                                        $"{entry.OriginalCurrency} {Math.Abs(entry.OriginalAmount):N2} × {entry.ExchangeRate:N4}")
                                        .FontSize(7).Italic();
                                }
                            });

                            // (3) Prihod od proizvoda
                            table.Cell().Border(1).Background(bgColor).Padding(4)
                                .AlignRight().Text(entry.ProductIncome != 0
                                    ? entry.ProductIncome.ToString("N2") : "");

                            // (4) Prihod od usluga
                            table.Cell().Border(1).Background(bgColor).Padding(4)
                                .AlignRight().Text(entry.ServiceIncome != 0
                                    ? entry.ServiceIncome.ToString("N2") : "");

                            // (5) Svega
                            table.Cell().Border(1).Background(bgColor).Padding(4)
                                .AlignRight().Text(entry.TotalIncome.ToString("N2"));
                        }

                        // UKUPNO red
                        var totalProduct = entries.Sum(x => x.ProductIncome);
                        var totalService = entries.Sum(x => x.ServiceIncome);
                        var grandTotal = entries.Sum(x => x.TotalIncome);

                        table.Cell().ColumnSpan(2).Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                            .AlignRight().Text("UKUPNO:").Bold();
                        table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                            .AlignRight().Text(totalProduct.ToString("N2")).Bold();
                        table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                            .AlignRight().Text(totalService.ToString("N2")).Bold();
                        table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                            .AlignRight().Text(grandTotal.ToString("N2")).Bold();
                    });

                    // Footer — potpis blok
                    page.Footer().PaddingTop(20).Row(row =>
                    {
                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().Text("Potpis: ___________________").FontSize(10);
                        });
                        row.RelativeColumn().AlignRight().Column(col =>
                        {
                            col.Item().Text($"Datum: {DateTime.UtcNow:dd.MM.yyyy}").FontSize(10);
                        });
                    });
                });
            });
        }

        private Guid GetCompanyIdOrThrow()
        {
            var companyIdString = _currentUserService.GetCompany();

            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException("Nevalidan ID firme.");

            return companyId;
        }
    }
}
