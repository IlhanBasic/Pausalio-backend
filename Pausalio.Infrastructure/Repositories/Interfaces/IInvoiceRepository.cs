using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Infrastructure.Repositories.Interfaces
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<IList<Invoice>> FindInvoicesWithEntities(Expression<Func<Invoice, bool>> predicate);
        Task<Invoice?> FindInvoiceWithEntities(Expression<Func<Invoice, bool>> predicate);
    }
}
