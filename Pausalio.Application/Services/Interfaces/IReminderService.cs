using Pausalio.Application.DTOs.Reminder;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IReminderService
    {
        Task<List<ReminderToReturnDto>> GetAllAsync();
        Task<ReminderToReturnDto?> GetByIdAsync(Guid id);
        Task CreateAsync(AddReminderDto dto);
        Task UpdateAsync(Guid id, UpdateReminderDto dto);
        Task MarkCompletedAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}