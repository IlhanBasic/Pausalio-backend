using Microsoft.EntityFrameworkCore;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Persistence;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Enums;

namespace Pausalio.Infrastructure.Repositories.Implementations
{
    public class KpoEntryRepository : Repository<KpoEntry>, IKpoEntryRepository
    {
        private readonly PausalioDbContext _context;

        public KpoEntryRepository(PausalioDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> GetNextSequenceNumberAsync(Guid businessProfileId, int year)
        {
            // Ova metoda MORA biti pozvana unutar aktivne transakcije
            // da bi row-level locking garantovao gapless numbering.
            var maxSeq = await _context.KpoEntries
                .Where(x => x.BusinessProfileId == businessProfileId && x.Year == year)
                .MaxAsync(x => (int?)x.SequenceNumber) ?? 0;

            return maxSeq + 1;
        }

        public async Task<IReadOnlyList<KpoEntry>> GetByYearAsync(Guid businessProfileId, int year)
        {
            return await _context.KpoEntries
                .Where(x => x.BusinessProfileId == businessProfileId && x.Year == year)
                .OrderBy(x => x.SequenceNumber)
                .ToListAsync();
        }

        public async Task<KpoEntry?> FindRegularByInvoiceIdAsync(Guid businessProfileId, Guid invoiceId)
        {
            return await _context.KpoEntries
                .Where(x => x.BusinessProfileId == businessProfileId &&
                            x.InvoiceId == invoiceId &&
                            x.EntryType == KpoEntryType.Regular)
                .FirstOrDefaultAsync();
        }
    }
}
