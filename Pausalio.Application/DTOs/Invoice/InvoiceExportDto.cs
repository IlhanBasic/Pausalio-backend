using Pausalio.Application.DTOs.InvoiceItem;
using Pausalio.Shared.Enums;
namespace Pausalio.Application.DTOs.Invoice
{
    public class InvoiceExportDto
    {
        // Podaci firme
        public string BusinessName { get; set; } = null!;
        public string BusinessPIB { get; set; } = null!;
        public string BusinessMB { get; set; } = null!;
        public string BusinessAddress { get; set; } = null!;
        public string BusinessCity { get; set; } = null!;
        public string BusinessEmail { get; set; } = null!;
        public string? BusinessPhone { get; set; }
        public string? BusinessWebsite { get; set; }
        public string? BusinessLogo { get; set; }

        // Podaci klijenta
        public string ClientName { get; set; } = null!;
        public string? ClientPIB { get; set; }
        public string? ClientMB { get; set; }
        public string ClientAddress { get; set; } = null!;
        public string ClientCity { get; set; } = null!;
        public string ClientEmail { get; set; } = null!;
        public string? ClientPhone { get; set; }

        // Podaci fakture
        public string InvoiceNumber { get; set; } = null!;
        public DateTime IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public Currency Currency { get; set; }
        public string CurrencyDisplay { get; set; } = null!;
        public decimal ExchangeRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountRSD { get; set; }

        // Stavke
        public List<InvoiceItemExportDto> Items { get; set; } = new();
    }
}
