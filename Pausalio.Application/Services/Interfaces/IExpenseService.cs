using Pausalio.Application.DTOs.Expense;
using Pausalio.Shared.Enums;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseToReturnDto>> GetAllAsync();
        Task<IEnumerable<ExpenseToReturnDto>> GetByStatusAsync(ExpenseStatus status);
        Task<ExpenseToReturnDto?> GetByIdAsync(Guid id);
        Task<ExpenseToReturnDto> CreateAsync(AddExpenseDto dto);
        Task UpdateAsync(Guid id, UpdateExpenseDto dto);
        Task DeleteAsync(Guid id);
        Task ArchiveAsync(Guid id);
        Task<ExpenseSummaryDto> GetSummaryAsync();
    }
}