using AutoMapper;
using Pausalio.Application.DTOs.BusinessInvite;
using Pausalio.Application.Helpers;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.Application.Services.Implementations
{
    public class BusinessInviteService : IBusinessInviteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationHelper _localizationHelper;
        public BusinessInviteService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationHelper localizationHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationHelper = localizationHelper;
        }

        public async Task<BusinessInviteToReturnDto?> GetBusinessInviteByEmail(string email)
        {
            var businessInvite = await _unitOfWork.BusinessInviteRepository.FindFirstOrDefaultAsync(x => x.Email == email);
            if (businessInvite == null)
                return null;

            return _mapper.Map<BusinessInviteToReturnDto>(businessInvite);
        }

        public async Task RemoveInvite(Guid id)
        {
            var businessInvite = await _unitOfWork.BusinessInviteRepository.FindFirstOrDefaultAsync(x => x.Id == id);
            if (businessInvite == null)
                throw new KeyNotFoundException(_localizationHelper.InviteTokenDoesNotExist);
            _unitOfWork.BusinessInviteRepository.Remove(businessInvite);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<BusinessInviteToReturnDto> SendInvite(AddBusinessInviteDto invite, Guid ownerId, Guid businessId)
        {
            var businessInvite = _mapper.Map<BusinessInvite>(invite);
            businessInvite.ExpiresAt = DateTime.UtcNow.AddMinutes(60);
            businessInvite.Token = InviteTokenHelper.GenerateToken();
            businessInvite.IsUsed = false;
            businessInvite.CreatedAt = DateTime.UtcNow;
            businessInvite.CreatedById = ownerId;
            businessInvite.BusinessProfileId = businessId;

            await _unitOfWork.BusinessInviteRepository.AddAsync(businessInvite);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<BusinessInviteToReturnDto>(businessInvite);
        }
        public async Task<BusinessInviteToReturnDto?> GetBusinessInviteByEmailAndCompany(string email, Guid companyId)
        {
            var businessInvite = await _unitOfWork.BusinessInviteRepository
                .FindFirstOrDefaultAsync(x => x.Email == email &&
                                              x.BusinessProfileId == companyId &&
                                              x.ExpiresAt > DateTime.UtcNow &&
                                              !x.IsUsed);

            if (businessInvite == null)
                return null;

            return _mapper.Map<BusinessInviteToReturnDto>(businessInvite);
        }
    }
}
