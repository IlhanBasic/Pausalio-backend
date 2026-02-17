using Microsoft.EntityFrameworkCore;
using Pausalio.Domain.Entities;
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
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        private readonly PausalioDbContext _context;
        public PaymentRepository(PausalioDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Payment>> FindPaymentsWithEntitiesAsync(Expression<Func<Payment, bool>> predicate)
        {
            return await _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i!.Client)
                .Include(p => p.Invoice)
                    .ThenInclude(i => i!.Items)
                .Include(p => p.TaxObligation)
                .Include(p => p.Expense)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Payment?> FindPaymentWithEntitiesAsync(Expression<Func<Payment, bool>> predicate)
        {
            return await _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i!.Client)
                .Include(p => p.Invoice)
                    .ThenInclude(i => i!.Items)
                .Include(p => p.TaxObligation)
                .Include(p => p.Expense)
                .FirstOrDefaultAsync(predicate);
        }
    }
}
