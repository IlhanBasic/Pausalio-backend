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
    public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
    {
        private readonly PausalioDbContext _context;
        public UserProfileRepository(PausalioDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<UserProfile?> GetByEmailWithEntitiesAsync(string email)
        {
            return await _context.UserProfiles
                .Include(u => u.UserBusinessProfiles)
                    .ThenInclude(u => u.BusinessProfile)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

    }
}
