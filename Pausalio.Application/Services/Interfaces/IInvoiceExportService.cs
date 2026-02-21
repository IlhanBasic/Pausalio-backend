using Pausalio.Application.DTOs.Invoice;
namespace Pausalio.Application.Services.Interfaces
{
    public interface IInvoiceExportService
    {
        Task<InvoiceExportDto> GetExportDataAsync(Guid invoiceId);
        Task<string> GenerateHtmlAsync(Guid invoiceId);
        Task<byte[]> GeneratePdfAsync(Guid invoiceId);
        Task SendInvoiceAsync(Guid invoiceId, SendInvoiceDto dto);
    }
}
