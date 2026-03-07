using Microsoft.EntityFrameworkCore;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Extensions;
using Pausalio.Infrastructure.Persistence;
using Pausalio.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Infrastructure.Repositories.Implementations
{
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {
        private readonly PausalioDbContext _context;
        public InvoiceRepository(PausalioDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IList<Invoice>> FindInvoicesWithEntities(Expression<Func<Invoice, bool>> predicate)
        {
            return await _context.Invoices
                .AddIncludes()
                .ToListAsync();
        }

        public async Task<Invoice?> FindInvoiceWithEntities(Expression<Func<Invoice, bool>> predicate)
        {
            return await _context.Invoices
                .AddIncludes()
                .FirstOrDefaultAsync(predicate);
        }
        public async Task<IList<Invoice>> FindInvoicesWithEntitiesAsync(Expression<Func<Invoice, bool>> predicate)
        {
            return await _context.Invoices
                .AddIncludes()
                .ToListAsync();
        }

        public async Task<Invoice?> FindInvoiceWithDetailsAsync(Expression<Func<Invoice, bool>> predicate)
        {
            return await _context.Invoices
                .AddIncludes()
                .FirstOrDefaultAsync(predicate);
        }
    }
}
