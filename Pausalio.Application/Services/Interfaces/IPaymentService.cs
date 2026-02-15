using Pausalio.Application.DTOs.Payment;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentToReturnDto>> GetAllAsync();
        Task<IEnumerable<PaymentToReturnDto>> GetByTypeAsync(PaymentType paymentType);
        Task<PaymentToReturnDto?> GetByIdAsync(Guid id);
        Task<PaymentToReturnDto> CreateAsync(AddPaymentDto dto);
        Task UpdateAsync(Guid id, UpdatePaymentDto dto);
        Task DeleteAsync(Guid id);
        Task<PaymentSummaryDto> GetSummaryAsync();
    }
}
