using Pausalio.Application.DTOs.Invoice;
using Pausalio.Application.DTOs.InvoiceItem;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Helpers;
using Pausalio.Shared.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Pausalio.Application.Services.Implementations
{
    public class InvoiceExportService : IInvoiceExportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly IPdfFactoryService _pdfFactoryService;

        public InvoiceExportService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService,
            ILocalizationHelper localizationHelper,
            IPdfFactoryService pdfFactoryService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _localizationHelper = localizationHelper;
            _pdfFactoryService = pdfFactoryService;
        }

        // ── PDF Label Records ──
        private record InvoicePdfLabels(
            string Title, string IssuerLabel, string RecipientLabel,
            string IssueDateLabel, string DueDateLabel, string CurrencyLabel,
            string ReferenceLabel, string NameCol, string TypeCol,
            string QtyCol, string PriceCol, string TotalCol,
            string TotalLabel, string ExchangeRateLabel, string TotalRsdLabel,
            string GrandTotalLabel, string NoteLabel, string FooterGenerated,
            string ServiceType, string ProductType,
            string BankLabel, string AccountLabel, string PaymentInfoLabel,
            string PaymentReferenceLabel, string IbanLabel, string SwiftLabel);

        private static readonly Dictionary<Language, InvoicePdfLabels> _pdfLabels = new()
        {
            [Language.Serbian] = new(
                "FAKTURA", "IZDAVALAC", "PRIMALAC",
                "DATUM IZDAVANJA", "ROK PLAĆANJA", "VALUTA",
                "POZIV NA BROJ", "NAZIV", "TIP",
                "KOL.", "CENA", "UKUPNO",
                "Ukupno", "Kurs", "Ukupno (RSD)",
                "UKUPNO ZA UPLATU", "NAPOMENA", "Dokument generisan automatski — Paušalio",
                "Usluga", "Proizvod",
                "Banka", "Broj računa", "PODACI ZA UPLATU",
                "Poziv na broj", "IBAN", "SWIFT"),
            [Language.English] = new(
                "INVOICE", "ISSUER", "RECIPIENT",
                "ISSUE DATE", "DUE DATE", "CURRENCY",
                "PAYMENT REF.", "DESCRIPTION", "TYPE",
                "QTY", "UNIT PRICE", "TOTAL",
                "Total", "Exchange Rate", "Total (RSD)",
                "AMOUNT DUE", "NOTE", "Document generated automatically — Paušalio",
                "Service", "Product",
                "Bank", "Account No.", "PAYMENT DETAILS",
                "Payment Reference", "IBAN", "SWIFT/BIC")
        };

        public async Task<InvoiceExportDto> GetExportDataAsync(Guid invoiceId)
        {
            var companyIdString = _currentUserService.GetCompany();
            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            var invoice = await _unitOfWork.InvoiceRepository
                .FindInvoiceWithDetailsAsync(x => x.Id == invoiceId &&
                                                  x.BusinessProfileId == companyId &&
                                                  !x.IsDeleted);

            if (invoice == null)
                throw new KeyNotFoundException(_localizationHelper.InvoiceNotFound);

            var business = await _unitOfWork.BusinessProfileRepository
                .GetByIdAsync(companyId);

            if (business == null)
                throw new KeyNotFoundException(_localizationHelper.BusinessProfileNotFound);

            // Determine language from client type
            var lang = LanguageResolver.Resolve(invoice.Client.ClientType);

            // Find matching bank account by invoice currency
            var bankAccounts = await _unitOfWork.BankAccountRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && x.IsActive);

            var bankAccount = bankAccounts.FirstOrDefault(x => x.Currency == invoice.Currency)
                           ?? bankAccounts.FirstOrDefault();

            return new InvoiceExportDto
            {
                // Firma
                BusinessName = business.BusinessName,
                BusinessPIB = business.PIB,
                BusinessMB = business.MB,
                BusinessAddress = business.Address,
                BusinessCity = business.City,
                BusinessEmail = business.Email,
                BusinessPhone = business.Phone,
                BusinessWebsite = business.Website,
                BusinessLogo = business.CompanyLogo,

                // Klijent
                ClientName = invoice.Client.Name,
                ClientPIB = invoice.Client.PIB,
                ClientMB = invoice.Client.MB,
                ClientAddress = invoice.Client.Address,
                ClientCity = invoice.Client.City,
                ClientEmail = invoice.Client.Email,
                ClientPhone = invoice.Client.Phone,

                // Faktura
                InvoiceNumber = invoice.InvoiceNumber,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                ReferenceNumber = invoice.ReferenceNumber,
                Notes = invoice.Notes,
                Currency = invoice.Currency,
                CurrencyDisplay = invoice.Currency.ToString(),
                ExchangeRate = invoice.ExchangeRate,
                TotalAmount = invoice.TotalAmount,
                TotalAmountRSD = invoice.TotalAmountRSD,

                // Stavke
                Items = invoice.Items.Select(item => new InvoiceItemExportDto
                {
                    Name = item.Name,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    ItemTypeDisplay = item.ItemType == ItemType.Service ? "Usluga" : "Proizvod"
                }).ToList(),

                // Jezik
                Language = lang,

                // Bankarski podaci
                BusinessBankName = bankAccount?.BankName,
                BusinessAccountNumber = bankAccount?.AccountNumber,
                BusinessIBAN = bankAccount?.IBAN,
                BusinessSWIFT = bankAccount?.SWIFT
            };
        }

        public async Task<string> GenerateHtmlAsync(Guid invoiceId)
        {
            var data = await GetExportDataAsync(invoiceId);
            return BuildHtml(data);
        }

        public async Task<byte[]> GeneratePdfAsync(Guid invoiceId)
        {
            var data = await GetExportDataAsync(invoiceId);
            var labels = _pdfLabels[data.Language];
            var culture = data.Language == Language.English
                ? CultureInfo.GetCultureInfo("en-US")
                : CultureInfo.GetCultureInfo("sr-Latn");

            return _pdfFactoryService.GeneratePdf(() =>
            {
                return Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                        page.Content().Column(col =>
                        {
                            // HEADER
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(data.BusinessName)
                                        .FontSize(18).Bold().FontColor("#4f46e5");
                                    c.Item().Text($"PIB: {data.BusinessPIB} | MB: {data.BusinessMB}")
                                        .FontSize(9).FontColor("#6b7280");
                                    c.Item().Text($"{data.BusinessAddress}, {data.BusinessCity}")
                                        .FontSize(9).FontColor("#6b7280");
                                    c.Item().Text(data.BusinessEmail)
                                        .FontSize(9).FontColor("#6b7280");
                                    if (!string.IsNullOrEmpty(data.BusinessPhone))
                                        c.Item().Text($"Tel: {data.BusinessPhone}")
                                            .FontSize(9).FontColor("#6b7280");
                                });

                                row.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text(labels.Title).FontSize(26).Bold().FontColor("#4f46e5");
                                    c.Item().Text(data.InvoiceNumber).FontSize(11).FontColor("#6b7280");
                                });
                            });

                            col.Item().PaddingVertical(12).LineHorizontal(2).LineColor("#4f46e5");

                            // PARTIES
                            col.Item().PaddingBottom(20).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(labels.IssuerLabel).FontSize(8).Bold()
                                        .FontColor("#9ca3af").LetterSpacing(1);
                                    c.Item().Text(data.BusinessName).Bold().FontSize(12);
                                    c.Item().Text($"{data.BusinessAddress}, {data.BusinessCity}")
                                        .FontSize(9).FontColor("#6b7280");
                                    c.Item().Text(data.BusinessEmail).FontSize(9).FontColor("#6b7280");
                                });

                                row.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text(labels.RecipientLabel).FontSize(8).Bold()
                                        .FontColor("#9ca3af").LetterSpacing(1);
                                    c.Item().Text(data.ClientName).Bold().FontSize(12);
                                    c.Item().Text($"{data.ClientAddress}, {data.ClientCity}")
                                        .FontSize(9).FontColor("#6b7280");
                                    if (!string.IsNullOrEmpty(data.ClientPIB))
                                        c.Item().Text($"PIB: {data.ClientPIB}").FontSize(9).FontColor("#6b7280");
                                    if (!string.IsNullOrEmpty(data.ClientMB))
                                        c.Item().Text($"MB: {data.ClientMB}").FontSize(9).FontColor("#6b7280");
                                    c.Item().Text(data.ClientEmail).FontSize(9).FontColor("#6b7280");
                                });
                            });

                            // META
                            col.Item().PaddingBottom(20).Background("#f9fafb").Padding(12).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(labels.IssueDateLabel).FontSize(8).Bold().FontColor("#9ca3af");
                                    c.Item().Text(data.IssueDate.ToString(
                                        data.Language == Language.English ? "MMMM d, yyyy" : "dd.MM.yyyy", culture)).Bold();
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(labels.DueDateLabel).FontSize(8).Bold().FontColor("#9ca3af");
                                    c.Item().Text(data.DueDate.HasValue
                                        ? data.DueDate.Value.ToString(
                                            data.Language == Language.English ? "MMMM d, yyyy" : "dd.MM.yyyy", culture)
                                        : "—").Bold();
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(labels.CurrencyLabel).FontSize(8).Bold().FontColor("#9ca3af");
                                    c.Item().Text(data.CurrencyDisplay).Bold();
                                });
                                if (!string.IsNullOrEmpty(data.ReferenceNumber))
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text(labels.ReferenceLabel).FontSize(8).Bold().FontColor("#9ca3af");
                                        c.Item().Text(data.ReferenceNumber).Bold();
                                    });
                                }
                            });

                            // ITEMS TABLE
                            col.Item().PaddingBottom(16).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(24);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.ConstantColumn(40);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background("#4f46e5").Padding(8)
                                        .Text("#").FontColor("#fff").FontSize(9).Bold();
                                    header.Cell().Background("#4f46e5").Padding(8)
                                        .Text(labels.NameCol).FontColor("#fff").FontSize(9).Bold();
                                    header.Cell().Background("#4f46e5").Padding(8)
                                        .Text(labels.TypeCol).FontColor("#fff").FontSize(9).Bold();
                                    header.Cell().Background("#4f46e5").Padding(8)
                                        .Text(labels.QtyCol).FontColor("#fff").FontSize(9).Bold();
                                    header.Cell().Background("#4f46e5").Padding(8)
                                        .Text(labels.PriceCol).FontColor("#fff").FontSize(9).Bold();
                                    header.Cell().Background("#4f46e5").Padding(8)
                                        .Text(labels.TotalCol).FontColor("#fff").FontSize(9).Bold();
                                });

                                for (int i = 0; i < data.Items.Count; i++)
                                {
                                    var item = data.Items[i];
                                    var bg = i % 2 == 0 ? "#ffffff" : "#f9fafb";
                                    var itemTypeDisplay = item.ItemTypeDisplay == "Usluga" || item.ItemTypeDisplay == "Service"
                                        ? labels.ServiceType : labels.ProductType;

                                    table.Cell().Background(bg).Padding(8).Text($"{i + 1}").FontSize(9);
                                    table.Cell().Background(bg).Padding(8).Column(c =>
                                    {
                                        c.Item().Text(item.Name).FontSize(9).Bold();
                                        if (!string.IsNullOrEmpty(item.Description))
                                            c.Item().Text(item.Description).FontSize(8).FontColor("#9ca3af");
                                    });
                                    table.Cell().Background(bg).Padding(8)
                                        .Text(itemTypeDisplay).FontSize(9).FontColor("#6b7280");
                                    table.Cell().Background(bg).Padding(8)
                                        .Text(item.Quantity.ToString()).FontSize(9);
                                    table.Cell().Background(bg).Padding(8)
                                        .Text($"{item.UnitPrice:N2} {data.CurrencyDisplay}").FontSize(9);
                                    table.Cell().Background(bg).Padding(8)
                                        .Text($"{item.TotalPrice:N2} {data.CurrencyDisplay}").FontSize(9).Bold();
                                }
                            });

                            // TOTALS
                            col.Item().AlignRight().Width(280).Column(c =>
                            {
                                c.Item().BorderBottom(1).BorderColor("#f3f4f6").PaddingVertical(4).Row(r =>
                                {
                                    r.RelativeItem().Text($"{labels.TotalLabel} ({data.CurrencyDisplay})").FontColor("#6b7280");
                                    r.AutoItem().Text($"{data.TotalAmount:N2} {data.CurrencyDisplay}").Bold();
                                });

                                if (data.Currency != Currency.RSD)
                                {
                                    c.Item().BorderBottom(1).BorderColor("#f3f4f6").PaddingVertical(4).Row(r =>
                                    {
                                        r.RelativeItem().Text($"{labels.ExchangeRateLabel} ({data.CurrencyDisplay}/RSD)").FontColor("#6b7280");
                                        r.AutoItem().Text($"{data.ExchangeRate:N4}").Bold();
                                    });
                                    c.Item().BorderBottom(1).BorderColor("#f3f4f6").PaddingVertical(4).Row(r =>
                                    {
                                        r.RelativeItem().Text(labels.TotalRsdLabel).FontColor("#6b7280");
                                        r.AutoItem().Text($"{data.TotalAmountRSD:N2} RSD").Bold();
                                    });
                                }

                                c.Item().BorderTop(2).BorderColor("#4f46e5").PaddingTop(8).Row(r =>
                                {
                                    r.RelativeItem().Text(labels.GrandTotalLabel)
                                        .Bold().FontSize(13).FontColor("#4f46e5");
                                    r.AutoItem().Text($"{data.TotalAmount:N2} {data.CurrencyDisplay}")
                                        .Bold().FontSize(13).FontColor("#4f46e5");
                                });
                            });

                            // BANK ACCOUNT
                            if (!string.IsNullOrEmpty(data.BusinessBankName))
                            {
                                col.Item().PaddingTop(16).Background("#f0f9ff")
                                    .Padding(12).Column(c =>
                                    {
                                        c.Item().Text(labels.PaymentInfoLabel)
                                            .FontSize(8).Bold().FontColor("#0ea5e9").LetterSpacing(1);
                                        c.Item().PaddingTop(4).Text($"{labels.BankLabel}: {data.BusinessBankName}")
                                            .FontSize(9).FontColor("#334155");
                                        c.Item().Text($"{labels.AccountLabel}: {data.BusinessAccountNumber}")
                                            .FontSize(9).FontColor("#334155");
                                        if (!string.IsNullOrEmpty(data.BusinessIBAN))
                                            c.Item().Text($"{labels.IbanLabel}: {data.BusinessIBAN}")
                                                .FontSize(9).FontColor("#334155");
                                        if (!string.IsNullOrEmpty(data.BusinessSWIFT))
                                            c.Item().Text($"{labels.SwiftLabel}: {data.BusinessSWIFT}")
                                                .FontSize(9).FontColor("#334155");
                                        if (!string.IsNullOrEmpty(data.ReferenceNumber))
                                            c.Item().Text($"{labels.PaymentReferenceLabel}: {data.ReferenceNumber}")
                                                .FontSize(9).FontColor("#334155");
                                    });
                            }

                            // NOTES
                            if (!string.IsNullOrEmpty(data.Notes))
                            {
                                col.Item().PaddingTop(16).Background("#fef9c3")
                                    .Padding(12).Row(row =>
                                    {
                                        row.ConstantItem(4).Background("#f59e0b");
                                        row.RelativeItem().PaddingLeft(8).Column(c =>
                                        {
                                            c.Item().Text(labels.NoteLabel)
                                                .FontSize(8).Bold().FontColor("#92400e");
                                            c.Item().Text(data.Notes)
                                                .FontSize(10).FontColor("#78350f");
                                        });
                                    });
                            }
                        });

                        // FOOTER
                        page.Footer().AlignCenter().Column(c =>
                        {
                            c.Item().LineHorizontal(1).LineColor("#e5e7eb");
                            c.Item().PaddingTop(8).Text(
                                $"{data.BusinessName} | PIB: {data.BusinessPIB} | {data.BusinessEmail}")
                                .FontSize(8).FontColor("#9ca3af");
                            c.Item().Text(labels.FooterGenerated)
                                .FontSize(8).FontColor("#9ca3af");
                        });
                    });
                });
            });
        }

        public async Task SendInvoiceAsync(Guid invoiceId, SendInvoiceDto dto)
        {
            if (dto.Emails == null || !dto.Emails.Any())
                throw new InvalidOperationException(_localizationHelper.NoEmailsProvided);

            var data = await GetExportDataAsync(invoiceId);
            var pdfBytes = await GeneratePdfAsync(invoiceId);

            var lang = data.Language;
            var subject = lang == Language.English
                ? $"Invoice {data.InvoiceNumber} — {data.BusinessName}"
                : $"Faktura {data.InvoiceNumber} — {data.BusinessName}";

            var body = _emailTemplateService.GetInvoiceEmailTemplate(data, lang);

            var attachmentName = lang == Language.English
                ? $"Invoice_{data.InvoiceNumber}.pdf"
                : $"Faktura_{data.InvoiceNumber}.pdf";

            foreach (var email in dto.Emails)
            {
                await _emailService.SendEmailWithAttachmentAsync(
                    email,
                    subject,
                    body,
                    pdfBytes,
                    attachmentName
                );
            }
        }

        // HTML za preview na frontendu — lokalizovan po jeziku klijenta
        private string BuildHtml(InvoiceExportDto data)
        {
            var lang = data.Language;
            var templateName = lang == Language.English ? "InvoiceTemplate_en.html" : "InvoiceTemplate.html";
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(assemblyFolder!, "Templates", templateName);

            // Fallback to Serbian if English template doesn't exist
            if (!File.Exists(path) && lang == Language.English)
            {
                path = Path.Combine(assemblyFolder!, "Templates", "InvoiceTemplate.html");
            }

            if (!File.Exists(path))
                throw new FileNotFoundException($"Invoice template not found: {path}");

            var template = File.ReadAllText(path);
            var labels = _pdfLabels[lang];
            var culture = lang == Language.English
                ? CultureInfo.GetCultureInfo("en-US")
                : CultureInfo.GetCultureInfo("sr-Latn");

            // Item rows
            var itemRows = new StringBuilder();
            for (int i = 0; i < data.Items.Count; i++)
            {
                var item = data.Items[i];
                var itemTypeDisplay = item.ItemTypeDisplay == "Usluga" || item.ItemTypeDisplay == "Service"
                    ? labels.ServiceType : labels.ProductType;

                itemRows.AppendLine($@"
                <tr>
                    <td>{i + 1}</td>
                    <td>
                        {item.Name}
                        {(string.IsNullOrEmpty(item.Description) ? "" : $"<div class='item-description'>{item.Description}</div>")}
                    </td>
                    <td>{itemTypeDisplay}</td>
                    <td>{item.Quantity}</td>
                    <td>{item.UnitPrice:N2} {data.CurrencyDisplay}</td>
                    <td>{item.TotalPrice:N2} {data.CurrencyDisplay}</td>
                </tr>");
            }

            var exchangeRateRow = data.Currency != Currency.RSD
                ? $@"<div class='totals-row'>
                        <span class='label'>{labels.ExchangeRateLabel} ({data.CurrencyDisplay}/RSD)</span>
                        <span>{data.ExchangeRate:N4}</span>
                     </div>
                     <div class='totals-row'>
                        <span class='label'>{labels.TotalRsdLabel}</span>
                        <span>{data.TotalAmountRSD:N2} RSD</span>
                     </div>"
                : "";

            var dateFormat = lang == Language.English ? "MMMM d, yyyy" : "dd.MM.yyyy";

            template = template
                .Replace("{{BusinessLogo}}", data.BusinessLogo ?? "")
                .Replace("{{BusinessName}}", data.BusinessName)
                .Replace("{{BusinessPIB}}", data.BusinessPIB)
                .Replace("{{BusinessMB}}", data.BusinessMB)
                .Replace("{{BusinessAddress}}", data.BusinessAddress)
                .Replace("{{BusinessCity}}", data.BusinessCity)
                .Replace("{{BusinessEmail}}", data.BusinessEmail)
                .Replace("{{BusinessPhone}}", data.BusinessPhone ?? "")
                .Replace("{{BusinessWebsite}}", data.BusinessWebsite ?? "")
                .Replace("{{ClientName}}", data.ClientName)
                .Replace("{{ClientPIB}}", data.ClientPIB ?? "")
                .Replace("{{ClientMB}}", data.ClientMB ?? "")
                .Replace("{{ClientAddress}}", data.ClientAddress)
                .Replace("{{ClientCity}}", data.ClientCity)
                .Replace("{{ClientEmail}}", data.ClientEmail)
                .Replace("{{ClientPhone}}", data.ClientPhone ?? "")
                .Replace("{{InvoiceNumber}}", data.InvoiceNumber)
                .Replace("{{IssueDate}}", data.IssueDate.ToString(dateFormat, culture))
                .Replace("{{DueDate}}", data.DueDate.HasValue ? data.DueDate.Value.ToString(dateFormat, culture) : "—")
                .Replace("{{CurrencyDisplay}}", data.CurrencyDisplay)
                .Replace("{{ReferenceNumber}}", data.ReferenceNumber ?? "")
                .Replace("{{Notes}}", data.Notes ?? "")
                .Replace("{{ItemRows}}", itemRows.ToString())
                .Replace("{{TotalAmount}}", data.TotalAmount.ToString("N2"))
                .Replace("{{TotalAmountRSD}}", data.TotalAmountRSD.ToString("N2"))
                .Replace("{{ExchangeRate}}", data.ExchangeRate.ToString("N4"))
                .Replace("{{ExchangeRateRows}}", exchangeRateRow)
                .Replace("{{ShowExchangeRate}}", (data.Currency != Currency.RSD).ToString().ToLower())
                .Replace("{{BusinessBankName}}", data.BusinessBankName ?? "")
                .Replace("{{BusinessAccountNumber}}", data.BusinessAccountNumber ?? "")
                .Replace("{{BusinessIBAN}}", data.BusinessIBAN ?? "")
                .Replace("{{BusinessSWIFT}}", data.BusinessSWIFT ?? "");

            template = HandleConditionals(template, "BusinessLogo", !string.IsNullOrEmpty(data.BusinessLogo));
            template = HandleConditionals(template, "BusinessPhone", !string.IsNullOrEmpty(data.BusinessPhone));
            template = HandleConditionals(template, "BusinessWebsite", !string.IsNullOrEmpty(data.BusinessWebsite));
            template = HandleConditionals(template, "ClientPIB", !string.IsNullOrEmpty(data.ClientPIB));
            template = HandleConditionals(template, "ClientMB", !string.IsNullOrEmpty(data.ClientMB));
            template = HandleConditionals(template, "ClientPhone", !string.IsNullOrEmpty(data.ClientPhone));
            template = HandleConditionals(template, "ReferenceNumber", !string.IsNullOrEmpty(data.ReferenceNumber));
            template = HandleConditionals(template, "Notes", !string.IsNullOrEmpty(data.Notes));
            template = HandleConditionals(template, "ShowExchangeRate", data.Currency != Currency.RSD);
            template = HandleConditionals(template, "HasBankAccount", !string.IsNullOrEmpty(data.BusinessBankName));
            template = HandleConditionals(template, "HasIBAN", !string.IsNullOrEmpty(data.BusinessIBAN));
            template = HandleConditionals(template, "HasSWIFT", !string.IsNullOrEmpty(data.BusinessSWIFT));

            return template;
        }

        // Helper za {{#if}} / {{/if}} blokove u HTML templatu
        private string HandleConditionals(string template, string key, bool condition)
        {
            var ifTag = $"{{{{#if {key}}}}}";
            var endTag = $"{{{{/if}}}}";

            while (template.Contains(ifTag))
            {
                var start = template.IndexOf(ifTag);
                var end = template.IndexOf(endTag, start) + endTag.Length;
                var block = template.Substring(start, end - start);
                var inner = block.Replace(ifTag, "").Replace(endTag, "");

                template = template.Replace(block, condition ? inner : "");
            }

            return template;
        }
    }
}