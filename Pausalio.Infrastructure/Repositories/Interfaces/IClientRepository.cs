using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Infrastructure.Repositories.Interfaces
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<IList<Client>> GetAllClientsWithEntities(Expression<Func<Client, bool>> predicate);
    }
}
