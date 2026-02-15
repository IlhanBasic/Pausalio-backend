using Pausalio.Application.DTOs.TaxObligation;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Interfaces
{
    public interface ITaxObligationService
    {
        Task<IEnumerable<TaxObligationToReturnDto>> GetAllAsync();
        Task<IEnumerable<TaxObligationToReturnDto>> GetByYearAsync(int year);
        Task<TaxObligationToReturnDto?> GetByYearAndMonthAsync(int year, int month);
        Task<IEnumerable<TaxObligationToReturnDto>> GetByStatusAsync(TaxObligationStatus status);
        Task<TaxObligationToReturnDto?> GetByIdAsync(Guid id);

        // 🆕 Automatsko generisanje
        Task<IEnumerable<TaxObligationToReturnDto>> GenerateAnnualObligationsAsync(GenerateTaxObligationsDto dto);

        // Ručno dodavanje
        Task<TaxObligationToReturnDto> CreateAsync(AddTaxObligationDto dto);

        Task UpdateAsync(Guid id, UpdateTaxObligationDto dto);
        Task DeleteAsync(Guid id);
        Task MarkAsPaidAsync(Guid id);
        Task<TaxObligationSummaryDto> GetSummaryAsync(int? year);
    }
}
