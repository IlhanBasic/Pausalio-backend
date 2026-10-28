using Pausalio.Application.DTOs.KpoBook;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IKpoBookService
    {
        /// <summary>Automatsko kreiranje KPO zapisa za fakturisanu fakturu.</summary>
        Task<KpoEntryToReturnDto> CreateEntryForInvoiceAsync(Guid invoiceId);

        /// <summary>Storno zapis (kad se faktura otkaže).</summary>
        Task<KpoEntryToReturnDto> CreateStornoEntryAsync(Guid invoiceId);

        /// <summary>Sve stavke za godinu.</summary>
        Task<IEnumerable<KpoEntryToReturnDto>> GetEntriesByYearAsync(int year);

        /// <summary>Pojedinačna stavka.</summary>
        Task<KpoEntryToReturnDto?> GetByIdAsync(Guid id);

        /// <summary>Godišnji sumar sa limitima i alertima.</summary>
        Task<KpoYearlySummaryDto> GetYearlySummaryAsync(int year);

        /// <summary>PDF eksport cele godine u 5-kolonskom formatu.</summary>
        Task<byte[]> ExportPdfAsync(int year);

        /// <summary>Popunjavanje KPO za postojeće fakture koje nemaju zapise.</summary>
        Task<int> BackfillFromExistingInvoicesAsync(int year);
    }
}
