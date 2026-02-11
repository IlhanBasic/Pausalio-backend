using AutoMapper;
using Pausalio.Application.DTOs.UserProfile;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;

namespace Pausalio.Application.Services.Implementations
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public UserProfileService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task<UserProfileToReturnDto?> CreateUserProfile(AddUserProfileDto userProfileDto)
        {
            var userProfile = _mapper.Map<UserProfile>(userProfileDto);

            if (userProfile == null)
            {
                return null;
            }

            await _unitOfWork.UserProfileRepository.AddAsync(userProfile);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserProfileToReturnDto>(userProfile);
        }

    }
}
