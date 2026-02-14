using Pausalio.Application.DTOs.ActivityCode;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IActivityCodeService
    {
        Task<List<ActivityCodeToReturnDto>> GetAllAsync();
        Task<ActivityCodeToReturnDto?> GetByIdAsync(Guid id);
        Task CreateAsync(AddActivityCodeDto dto);
        Task UpdateAsync(Guid id, UpdateActivityCodeDto dto);
        Task DeleteAsync(Guid id);
    }
}