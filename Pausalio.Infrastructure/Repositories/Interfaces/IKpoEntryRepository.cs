using Pausalio.Domain.Entities;
using System.Linq.Expressions;

namespace Pausalio.Infrastructure.Repositories.Interfaces
{
    public interface IKpoEntryRepository : IRepository<KpoEntry>
    {
        /// <summary>
        /// Vraća sledeći redni broj za dati biznis i godinu.
        /// MORA se pozivati unutar aktivne transakcije da bi se garantovao gapless numbering.
        /// </summary>
        Task<int> GetNextSequenceNumberAsync(Guid businessProfileId, int year);

        /// <summary>
        /// Vraća sve KPO zapise za datu godinu, sortirane po rednom broju.
        /// </summary>
        Task<IReadOnlyList<KpoEntry>> GetByYearAsync(Guid businessProfileId, int year);

        /// <summary>
        /// Nalazi KPO zapis za datu fakturu (regularni, ne storno).
        /// </summary>
        Task<KpoEntry?> FindRegularByInvoiceIdAsync(Guid businessProfileId, Guid invoiceId);
    }
}
