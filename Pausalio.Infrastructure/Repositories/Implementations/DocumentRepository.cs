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
    public class DocumentRepository : Repository<Document>, IDocumentRepository
    {
        private readonly PausalioDbContext _context;
        public DocumentRepository(PausalioDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
