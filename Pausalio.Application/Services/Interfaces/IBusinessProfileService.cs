using Pausalio.Application.DTOs.BusinessProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IBusinessProfileService
    {
        Task<BusinessProfileToReturnDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<BusinessProfileToReturnDto>> GetAllAsync();
        Task<IEnumerable<BusinessProfileToReturnDto>> GetAllWithEntitiesAsync();
        Task<IEnumerable<BusinessProfileToReturnDto>> GetUserBusinessProfilesAsync(Guid userId);
        Task UpdateAsync(Guid id, UpdateBusinessProfileDto dto);
        Task DeactivateAsync(Guid id);
        Task ActivateAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}
