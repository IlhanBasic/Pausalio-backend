using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Infrastructure.Repositories.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> FindPaymentsWithEntitiesAsync(Expression<Func<Payment, bool>> predicate);
        Task<Payment?> FindPaymentWithEntitiesAsync(Expression<Func<Payment, bool>> predicate);
    }
}
