using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Persistence;
using Pausalio.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Infrastructure.Repositories.Implementations
{
    public class AiMessageRepository : Repository<AiMessage>, IAiMessageRepository
    {
        private readonly PausalioDbContext _context;
        public AiMessageRepository(PausalioDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
