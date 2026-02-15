using Pausalio.Application.DTOs.Client;
using Pausalio.Application.DTOs.Country;
using Pausalio.Shared.Enums;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IClientService
    {
        Task<IEnumerable<ClientToReturnDto>> GetAllAsync();
        Task<IEnumerable<ClientToReturnDto>> GetByTypeAsync(ClientType clientType);
        Task<ClientToReturnDto?> GetByIdAsync(Guid id);
        Task CreateAsync(AddClientDto dto);
        Task UpdateAsync(Guid id, UpdateClientDto dto);
        Task DeleteAsync(Guid id);
        Task ActivateAsync(Guid id);
    }
}