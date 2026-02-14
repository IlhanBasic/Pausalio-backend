using AutoMapper;
using Pausalio.Application.DTOs.ActivityCode;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Services.Implementations
{
    public class ActivityCodeService : IActivityCodeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationHelper _localizationHelper;

        public ActivityCodeService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILocalizationHelper localizationHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationHelper = localizationHelper;
        }

        public async Task<List<ActivityCodeToReturnDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.ActivityCodeRepository.GetAllAsync();
            return _mapper.Map<List<ActivityCodeToReturnDto>>(entities);
        }

        public async Task<ActivityCodeToReturnDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.ActivityCodeRepository.GetByIdAsync(id);

            if (entity == null)
                return null;

            return _mapper.Map<ActivityCodeToReturnDto>(entity);
        }

        public async Task CreateAsync(AddActivityCodeDto dto)
        {
            var exists = await _unitOfWork.ActivityCodeRepository
                .FindFirstOrDefaultAsync(x => x.Code == dto.Code);

            if (exists != null)
                throw new InvalidOperationException(_localizationHelper.ActivityCodeAlreadyExists);

            var entity = _mapper.Map<ActivityCode>(dto);

            await _unitOfWork.ActivityCodeRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, UpdateActivityCodeDto dto)
        {
            var entity = await _unitOfWork.ActivityCodeRepository.GetByIdAsync(id);

            if (entity == null)
                throw new KeyNotFoundException(_localizationHelper.ActivityCodeNotFound);

            var duplicate = await _unitOfWork.ActivityCodeRepository
                .FindFirstOrDefaultAsync(x => x.Code == dto.Code && x.Id != id);

            if (duplicate != null)
                throw new InvalidOperationException(_localizationHelper.ActivityCodeAlreadyExists);

            _mapper.Map(dto, entity);

            _unitOfWork.ActivityCodeRepository.Update(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.ActivityCodeRepository.GetByIdAsync(id);

            if (entity == null)
                throw new KeyNotFoundException(_localizationHelper.ActivityCodeNotFound);

            _unitOfWork.ActivityCodeRepository.Remove(entity);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}