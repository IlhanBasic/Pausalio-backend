using Pausalio.Application.DTOs.Item;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IItemService
    {
        Task<List<ItemToReturnDto>> GetAllAsync();
        Task<ItemToReturnDto?> GetByIdAsync(Guid id);
        Task CreateAsync(AddItemDto dto);
        Task UpdateAsync(Guid id, UpdateItemDto dto);
        Task DeleteAsync(Guid id);
    }
}