using Pausalio.Application.DTOs.Invoice;
using Pausalio.Application.DTOs.InvoiceItem;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection;
using System.Text;

namespace Pausalio.Application.Services.Implementations
{
    public class InvoiceExportService : IInvoiceExportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmailService _emailService;
        private readonly ILocalizationHelper _localizationHelper;

        public InvoiceExportService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IEmailService emailService,
            ILocalizationHelper localizationHelper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _emailService = emailService;
            _localizationHelper = localizationHelper;
        }

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
                }).ToList()
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

            var document = Document.Create(container =>
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
                                c.Item().Text("FAKTURA").FontSize(26).Bold().FontColor("#4f46e5");
                                c.Item().Text(data.InvoiceNumber).FontSize(11).FontColor("#6b7280");
                            });
                        });

                        col.Item().PaddingVertical(12).LineHorizontal(2).LineColor("#4f46e5");

                        // PARTIES
                        col.Item().PaddingBottom(20).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("IZDAVALAC").FontSize(8).Bold()
                                    .FontColor("#9ca3af").LetterSpacing(1);
                                c.Item().Text(data.BusinessName).Bold().FontSize(12);
                                c.Item().Text($"{data.BusinessAddress}, {data.BusinessCity}")
                                    .FontSize(9).FontColor("#6b7280");
                                c.Item().Text(data.BusinessEmail).FontSize(9).FontColor("#6b7280");
                            });

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("PRIMALAC").FontSize(8).Bold()
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
                                c.Item().Text("DATUM IZDAVANJA").FontSize(8).Bold().FontColor("#9ca3af");
                                c.Item().Text(data.IssueDate.ToString("dd.MM.yyyy")).Bold();
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("ROK PLAĆANJA").FontSize(8).Bold().FontColor("#9ca3af");
                                c.Item().Text(data.DueDate.HasValue
                                    ? data.DueDate.Value.ToString("dd.MM.yyyy")
                                    : "—").Bold();
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("VALUTA").FontSize(8).Bold().FontColor("#9ca3af");
                                c.Item().Text(data.CurrencyDisplay).Bold();
                            });
                            if (!string.IsNullOrEmpty(data.ReferenceNumber))
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("POZIV NA BROJ").FontSize(8).Bold().FontColor("#9ca3af");
                                    c.Item().Text(data.ReferenceNumber).Bold();
                                });
                            }
                        });

                        // ITEMS TABLE
                        col.Item().PaddingBottom(16).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(24);  // #
                                columns.RelativeColumn(3);   // Naziv
                                columns.RelativeColumn(1);   // Tip
                                columns.ConstantColumn(40);  // Kol
                                columns.RelativeColumn(1);   // Cena
                                columns.RelativeColumn(1);   // Ukupno
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background("#4f46e5").Padding(8)
                                    .Text("#").FontColor("#fff").FontSize(9).Bold();
                                header.Cell().Background("#4f46e5").Padding(8)
                                    .Text("NAZIV").FontColor("#fff").FontSize(9).Bold();
                                header.Cell().Background("#4f46e5").Padding(8)
                                    .Text("TIP").FontColor("#fff").FontSize(9).Bold();
                                header.Cell().Background("#4f46e5").Padding(8)
                                    .Text("KOL.").FontColor("#fff").FontSize(9).Bold();
                                header.Cell().Background("#4f46e5").Padding(8)
                                    .Text("CENA").FontColor("#fff").FontSize(9).Bold();
                                header.Cell().Background("#4f46e5").Padding(8)
                                    .Text("UKUPNO").FontColor("#fff").FontSize(9).Bold();
                            });

                            // Rows
                            for (int i = 0; i < data.Items.Count; i++)
                            {
                                var item = data.Items[i];
                                var bg = i % 2 == 0 ? "#ffffff" : "#f9fafb";

                                table.Cell().Background(bg).Padding(8).Text($"{i + 1}").FontSize(9);
                                table.Cell().Background(bg).Padding(8).Column(c =>
                                {
                                    c.Item().Text(item.Name).FontSize(9).Bold();
                                    if (!string.IsNullOrEmpty(item.Description))
                                        c.Item().Text(item.Description).FontSize(8).FontColor("#9ca3af");
                                });
                                table.Cell().Background(bg).Padding(8)
                                    .Text(item.ItemTypeDisplay).FontSize(9).FontColor("#6b7280");
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
                                r.RelativeItem().Text($"Ukupno ({data.CurrencyDisplay})").FontColor("#6b7280");
                                r.AutoItem().Text($"{data.TotalAmount:N2} {data.CurrencyDisplay}").Bold();
                            });

                            if (data.Currency != Currency.RSD)
                            {
                                c.Item().BorderBottom(1).BorderColor("#f3f4f6").PaddingVertical(4).Row(r =>
                                {
                                    r.RelativeItem().Text($"Kurs ({data.CurrencyDisplay}/RSD)").FontColor("#6b7280");
                                    r.AutoItem().Text($"{data.ExchangeRate:N4}").Bold();
                                });
                                c.Item().BorderBottom(1).BorderColor("#f3f4f6").PaddingVertical(4).Row(r =>
                                {
                                    r.RelativeItem().Text("Ukupno (RSD)").FontColor("#6b7280");
                                    r.AutoItem().Text($"{data.TotalAmountRSD:N2} RSD").Bold();
                                });
                            }

                            c.Item().BorderTop(2).BorderColor("#4f46e5").PaddingTop(8).Row(r =>
                            {
                                r.RelativeItem().Text("UKUPNO ZA UPLATU")
                                    .Bold().FontSize(13).FontColor("#4f46e5");
                                r.AutoItem().Text($"{data.TotalAmount:N2} {data.CurrencyDisplay}")
                                    .Bold().FontSize(13).FontColor("#4f46e5");
                            });
                        });

                        // NOTES
                        if (!string.IsNullOrEmpty(data.Notes))
                        {
                            col.Item().PaddingTop(16).Background("#fef9c3")
                                .BorderLeft(4).BorderColor("#f59e0b")
                                .Padding(12).Column(c =>
                                {
                                    c.Item().Text("NAPOMENA").FontSize(8).Bold().FontColor("#92400e");
                                    c.Item().Text(data.Notes).FontSize(10).FontColor("#78350f");
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
                        c.Item().Text("Dokument generisan automatski — Paušalio")
                            .FontSize(8).FontColor("#9ca3af");
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task SendInvoiceAsync(Guid invoiceId, SendInvoiceDto dto)
        {
            if (dto.Emails == null || !dto.Emails.Any())
                throw new InvalidOperationException(_localizationHelper.NoEmailsProvided);

            var data = await GetExportDataAsync(invoiceId);
            var pdfBytes = await GeneratePdfAsync(invoiceId);
            var subject = $"Faktura {data.InvoiceNumber} — {data.BusinessName}";

            // Email body
            var body = BuildEmailBody(data);

            foreach (var email in dto.Emails)
            {
                await _emailService.SendEmailWithAttachmentAsync(
                    email,
                    subject,
                    body,
                    pdfBytes,
                    $"Faktura_{data.InvoiceNumber}.pdf"
                );
            }
        }

        // HTML za preview na frontendu
        private string BuildHtml(InvoiceExportDto data)
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(assemblyFolder!, "Templates", "InvoiceTemplate.html");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Invoice template not found: {path}");

            var template = File.ReadAllText(path);

            // Item rows
            var itemRows = new StringBuilder();
            for (int i = 0; i < data.Items.Count; i++)
            {
                var item = data.Items[i];
                itemRows.AppendLine($@"
                <tr>
                    <td>{i + 1}</td>
                    <td>
                        {item.Name}
                        {(string.IsNullOrEmpty(item.Description) ? "" : $"<div class='item-description'>{item.Description}</div>")}
                    </td>
                    <td>{item.ItemTypeDisplay}</td>
                    <td>{item.Quantity}</td>
                    <td>{item.UnitPrice:N2} {data.CurrencyDisplay}</td>
                    <td>{item.TotalPrice:N2} {data.CurrencyDisplay}</td>
                </tr>");
            }

            var exchangeRateRow = data.Currency != Currency.RSD
                ? $@"<div class='totals-row'>
                        <span class='label'>Kurs ({data.CurrencyDisplay}/RSD)</span>
                        <span>{data.ExchangeRate:N4}</span>
                     </div>
                     <div class='totals-row'>
                        <span class='label'>Ukupno (RSD)</span>
                        <span>{data.TotalAmountRSD:N2} RSD</span>
                     </div>"
                : "";

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
                .Replace("{{IssueDate}}", data.IssueDate.ToString("dd.MM.yyyy"))
                .Replace("{{DueDate}}", data.DueDate.HasValue ? data.DueDate.Value.ToString("dd.MM.yyyy") : "—")
                .Replace("{{CurrencyDisplay}}", data.CurrencyDisplay)
                .Replace("{{ReferenceNumber}}", data.ReferenceNumber ?? "")
                .Replace("{{Notes}}", data.Notes ?? "")
                .Replace("{{ItemRows}}", itemRows.ToString())
                .Replace("{{TotalAmount}}", data.TotalAmount.ToString("N2"))
                .Replace("{{TotalAmountRSD}}", data.TotalAmountRSD.ToString("N2"))
                .Replace("{{ExchangeRate}}", data.ExchangeRate.ToString("N4"))
                .Replace("{{ExchangeRateRows}}", exchangeRateRow)
                .Replace("{{ShowExchangeRate}}", (data.Currency != Currency.RSD).ToString().ToLower());

            template = HandleConditionals(template, "BusinessLogo", !string.IsNullOrEmpty(data.BusinessLogo));
            template = HandleConditionals(template, "BusinessPhone", !string.IsNullOrEmpty(data.BusinessPhone));
            template = HandleConditionals(template, "BusinessWebsite", !string.IsNullOrEmpty(data.BusinessWebsite));
            template = HandleConditionals(template, "ClientPIB", !string.IsNullOrEmpty(data.ClientPIB));
            template = HandleConditionals(template, "ClientMB", !string.IsNullOrEmpty(data.ClientMB));
            template = HandleConditionals(template, "ClientPhone", !string.IsNullOrEmpty(data.ClientPhone));
            template = HandleConditionals(template, "ReferenceNumber", !string.IsNullOrEmpty(data.ReferenceNumber));
            template = HandleConditionals(template, "Notes", !string.IsNullOrEmpty(data.Notes));
            template = HandleConditionals(template, "ShowExchangeRate", data.Currency != Currency.RSD);

            return template;
        }

        private string BuildEmailBody(InvoiceExportDto data)
        {
            return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: #4f46e5; padding: 24px; border-radius: 8px 8px 0 0;'>
                    <h1 style='color: white; margin: 0; font-size: 20px;'>Nova faktura</h1>
                </div>
                <div style='background: #f9fafb; padding: 24px; border-radius: 0 0 8px 8px;'>
                    <p>Poštovani/a,</p>
                    <p>U prilogu se nalazi faktura <strong>{data.InvoiceNumber}</strong> od <strong>{data.BusinessName}</strong>.</p>
                    <table style='width: 100%; margin: 16px 0; border-collapse: collapse;'>
                        <tr>
                            <td style='padding: 8px; color: #6b7280;'>Broj fakture:</td>
                            <td style='padding: 8px; font-weight: bold;'>{data.InvoiceNumber}</td>
                        </tr>
                        <tr style='background: #fff;'>
                            <td style='padding: 8px; color: #6b7280;'>Datum izdavanja:</td>
                            <td style='padding: 8px;'>{data.IssueDate:dd.MM.yyyy}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; color: #6b7280;'>Rok plaćanja:</td>
                            <td style='padding: 8px;'>{(data.DueDate.HasValue ? data.DueDate.Value.ToString("dd.MM.yyyy") : "—")}</td>
                        </tr>
                        <tr style='background: #fff;'>
                            <td style='padding: 8px; color: #6b7280;'>Iznos:</td>
                            <td style='padding: 8px; font-weight: bold; color: #4f46e5;'>{data.TotalAmount:N2} {data.CurrencyDisplay}</td>
                        </tr>
                    </table>
                    <p style='color: #6b7280; font-size: 12px;'>Faktura je priložena kao PDF dokument.</p>
                    <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 16px 0;'>
                    <p style='color: #9ca3af; font-size: 11px;'>
                        {data.BusinessName} | {data.BusinessEmail}
                        {(string.IsNullOrEmpty(data.BusinessPhone) ? "" : $" | {data.BusinessPhone}")}
                    </p>
                </div>
            </div>";
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