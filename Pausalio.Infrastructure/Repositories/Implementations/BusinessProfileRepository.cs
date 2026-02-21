using Microsoft.EntityFrameworkCore;
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
    public class BusinessProfileRepository : Repository<BusinessProfile>, IBusinessProfileRepository
    {
        private readonly PausalioDbContext _context;
        public BusinessProfileRepository(PausalioDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<BusinessProfile?> GetCompanyByIdWithEntities(Guid id)
        {
            var businessProfile = await _context.BusinessProfiles.Include(x=>x.ActivityCode).FirstOrDefaultAsync(x => x.Id == id);
            return businessProfile;
        }
    }
}
