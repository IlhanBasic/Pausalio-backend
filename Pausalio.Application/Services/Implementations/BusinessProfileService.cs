using AutoMapper;
using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Implementations
{
    public class BusinessProfileService : IBusinessProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly ICurrentUserService _currentUserService;
        public BusinessProfileService(
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

        public async Task<BusinessProfileToReturnDto?> GetByIdAsync(Guid id)
        {
            var business = await _unitOfWork.BusinessProfileRepository
                .FindFirstOrDefaultAsync(x => x.Id == id && x.IsActive);

            if (business == null)
                return null;

            return _mapper.Map<BusinessProfileToReturnDto>(business);
        }

        public async Task<IEnumerable<BusinessProfileToReturnDto>> GetAllAsync()
        {
            var businesses = await _unitOfWork.BusinessProfileRepository
                .GetAllAsync();

            return _mapper.Map<IEnumerable<BusinessProfileToReturnDto>>(businesses);
        }

        public async Task<IEnumerable<BusinessProfileToReturnDto>> GetUserBusinessProfilesAsync(Guid userId)
        {
            var userBusinessProfiles = await _unitOfWork.UserBusinessProfileRepository
                .FindAllAsync(x => x.UserId == userId);

            var businessIds = userBusinessProfiles.Select(ubp => ubp.BusinessProfileId).ToList();

            var businesses = await _unitOfWork.BusinessProfileRepository
                .FindAllAsync(x => businessIds.Contains(x.Id) && x.IsActive);

            return _mapper.Map<IEnumerable<BusinessProfileToReturnDto>>(businesses);
        }

        public async Task UpdateAsync(Guid id, UpdateBusinessProfileDto dto)
        {
            var business = await _unitOfWork.BusinessProfileRepository
                .FindFirstOrDefaultAsync(x => x.Id == id);

            if (business == null)
                throw new KeyNotFoundException(_localizationHelper.BusinesProfileNotFound);

            if (business.PIB != dto.PIB || business.MB != dto.MB)
            {
                var existingBusiness = await _unitOfWork.BusinessProfileRepository
                    .FindFirstOrDefaultAsync(x => (x.PIB == dto.PIB || x.MB == dto.MB) && x.Id != id);

                if (existingBusiness != null)
                    throw new InvalidOperationException(_localizationHelper.CompanyWithPIBOrMBAlreadyExists);
            }

                     
            business.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(dto,business);
            _unitOfWork.BusinessProfileRepository.Update(business);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeactivateAsync(Guid id)
        {
            var business = await _unitOfWork.BusinessProfileRepository
                .FindFirstOrDefaultAsync(x => x.Id == id);

            if (business == null)
                throw new KeyNotFoundException(_localizationHelper.BusinesProfileNotFound);

            business.IsActive = false;
            business.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.BusinessProfileRepository.Update(business);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ActivateAsync(Guid id)
        {
            var business = await _unitOfWork.BusinessProfileRepository
                .FindFirstOrDefaultAsync(x => x.Id == id);

            if (business == null)
                throw new KeyNotFoundException(_localizationHelper.BusinesProfileNotFound);

            business.IsActive = true;
            business.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.BusinessProfileRepository.Update(business);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
