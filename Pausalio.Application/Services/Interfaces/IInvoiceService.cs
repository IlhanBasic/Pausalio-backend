using Pausalio.Application.DTOs.Invoice;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceToReturnDto>> GetAllAsync();
        Task<IEnumerable<InvoiceToReturnDto>> GetByStatusAsync(InvoiceStatus status);
        Task<IEnumerable<InvoiceToReturnDto>> GetByPaymentStatusAsync(PaymentStatus paymentStatus);
        Task<InvoiceToReturnDto?> GetByIdAsync(Guid id);
        Task<InvoiceToReturnDto> CreateAsync(AddInvoiceDto dto);
        Task UpdateAsync(Guid id, UpdateInvoiceDto dto);
        Task ArchiveInvoice(Guid id);
        Task CancelInvoice(Guid id);
        Task DeleteAsync(Guid id);
        Task<InvoiceSummaryDto> GetSummaryAsync();
        Task<string> GenerateInvoiceNumberAsync();
    }
}
