using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Infrastructure.Repositories.Interfaces
{
    public interface IBusinessProfileRepository : IRepository<BusinessProfile>
    {
        Task<BusinessProfile?> GetCompanyByIdWithEntities(Guid id);
    }
}
