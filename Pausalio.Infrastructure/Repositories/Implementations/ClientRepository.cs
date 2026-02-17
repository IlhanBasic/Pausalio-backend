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
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        private readonly PausalioDbContext _context;
        public ClientRepository(PausalioDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IList<Client>> GetAllClientsWithEntities(Expression<Func<Client, bool>> predicate)
        {
            var clients = await _context.Clients.Include(x=>x.Country).Where(predicate).ToListAsync();
            return clients;
        }
    }
}
