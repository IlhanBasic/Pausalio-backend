using AutoMapper;
using Pausalio.Application.DTOs.Reminder;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Services.Implementations
{
    public class ReminderService : IReminderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly ICurrentUserService _currentUserService;

        public ReminderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILocalizationHelper localizationHelper,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationHelper = localizationHelper;
            _currentUserService = currentUserService;
        }

        public async Task<List<ReminderToReturnDto>> GetAllAsync()
        {
            var companyId = GetCurrentCompanyId();

            var reminders = await _unitOfWork.ReminderRepository
                .FindAllAsync(x => x.BusinessProfileId == companyId && !x.IsDeleted);

            return _mapper.Map<List<ReminderToReturnDto>>(reminders);
        }

        public async Task<ReminderToReturnDto?> GetByIdAsync(Guid id)
        {
            var companyId = GetCurrentCompanyId();

            var reminder = await _unitOfWork.ReminderRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId && !x.IsDeleted);

            if (reminder == null)
                return null;

            return _mapper.Map<ReminderToReturnDto>(reminder);
        }

        public async Task CreateAsync(AddReminderDto dto)
        {
            var companyId = GetCurrentCompanyId();

            var entity = _mapper.Map<Reminder>(dto);
            entity.BusinessProfileId = companyId;
            entity.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.ReminderRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, UpdateReminderDto dto)
        {
            var companyId = GetCurrentCompanyId();

            var reminder = await _unitOfWork.ReminderRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId && !x.IsDeleted);

            if (reminder == null)
                throw new KeyNotFoundException(_localizationHelper.ReminderNotFound);

            _mapper.Map(dto, reminder);

            _unitOfWork.ReminderRepository.Update(reminder);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkCompletedAsync(Guid id)
        {
            var companyId = GetCurrentCompanyId();

            var reminder = await _unitOfWork.ReminderRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId && !x.IsDeleted);

            if (reminder == null)
                throw new KeyNotFoundException(_localizationHelper.ReminderNotFound);
            if (reminder.DueDate > DateTime.UtcNow)
                throw new Exception(_localizationHelper.ReminderMarkedFailed);
            reminder.IsCompleted = true;
            reminder.CompletedAt = DateTime.UtcNow;

            _unitOfWork.ReminderRepository.Update(reminder);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var companyId = GetCurrentCompanyId();

            var reminder = await _unitOfWork.ReminderRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.BusinessProfileId == companyId && !x.IsDeleted);

            if (reminder == null)
                throw new KeyNotFoundException(_localizationHelper.ReminderNotFound);

            reminder.IsDeleted = true;
            reminder.DeletedAt = DateTime.UtcNow;

            _unitOfWork.ReminderRepository.Update(reminder);
            await _unitOfWork.SaveChangesAsync();
        }

        private Guid GetCurrentCompanyId()
        {
            var companyIdString = _currentUserService.GetCompany();
            if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                throw new UnauthorizedAccessException(_localizationHelper.InvalidCompanyId);

            return companyId;
        }
    }
}