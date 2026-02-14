using Pausalio.Application.DTOs.BankAccount;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IBankAccountService
    {
        Task<List<BankAccountToReturnDto>> GetAllAsync();
        Task<BankAccountToReturnDto?> GetByIdAsync(Guid id);
        Task CreateAsync(AddBankAccountDto dto);
        Task UpdateAsync(Guid id, UpdateBankAccountDto dto);
        Task DeleteAsync(Guid id);
    }
}