using AutoMapper;
using Pausalio.Application.DTOs.BusinessInvite;
using Pausalio.Application.Helpers;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;

namespace Pausalio.Application.Services.Implementations
{
    public class BusinessInviteService : IBusinessInviteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public BusinessInviteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BusinessInviteToReturnDto?> GetBusinessInviteByEmail(string email)
        {
            var businessInvite = await _unitOfWork.BusinessInviteRepository.FindFirstOrDefaultAsync(x => x.Email == email);
            if (businessInvite == null)
                return null;

            return _mapper.Map<BusinessInviteToReturnDto>(businessInvite);
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
    }
}
