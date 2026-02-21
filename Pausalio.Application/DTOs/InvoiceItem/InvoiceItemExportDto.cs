namespace Pausalio.Application.DTOs.InvoiceItem
{
    public class InvoiceItemExportDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string ItemTypeDisplay { get; set; } = null!;
    }
}
