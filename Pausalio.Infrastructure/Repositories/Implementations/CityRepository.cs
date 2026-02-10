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
    public class CityRepository : Repository<City>, ICityRepository
    {
        private readonly PausalioDbContext _context;
        public CityRepository(PausalioDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
