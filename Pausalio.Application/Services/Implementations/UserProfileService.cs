using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Pausalio.Application.DTOs.BusinessInvite;
using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Application.DTOs.UserProfile;
using Pausalio.Application.Helpers;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;
using System.Runtime.InteropServices;

namespace Pausalio.Application.Services.Implementations
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalizationHelper _localizationHelper;

        public UserProfileService(ILocalizationHelper localizationHelper, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _localizationHelper = localizationHelper;
        }

        public async Task<UserProfileToReturnDto?> LoginAsync(string email, string password)
        {
            var user = await _unitOfWork.UserProfileRepository.GetByEmailWithEntitiesAsync(email);

            if (user == null)
                throw new Exception(_localizationHelper.UserNotFound);

            if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
                throw new Exception(_localizationHelper.InvalidPassword);

            if (!user.IsEmailVerified)
                throw new Exception(_localizationHelper.EmailNotVerified);

            if (!user.IsActive)
                throw new Exception(_localizationHelper.UserInactive);


            return _mapper.Map<UserProfileToReturnDto>(user);
        }


        public async Task<UserBusinessProfile> AddUserToBusinessProfile(Guid userId, Guid businessProfileId, UserRole role)
        {
            var userBusinessProfile = new UserBusinessProfile
            {
                UserId = userId,
                BusinessProfileId = businessProfileId,
                Role = role
            };

            await _unitOfWork.UserBusinessProfileRepository.AddAsync(userBusinessProfile);
            if (role == UserRole.Assistant)
            {
                var userProfile = await _unitOfWork.UserProfileRepository.GetByIdAsync(userId);
                var businessInvite = await _unitOfWork.BusinessInviteRepository.FindFirstOrDefaultAsync(x=>x.BusinessProfileId == businessProfileId && x.Email == userProfile!.Email);
                if (businessInvite != null)
                {
                    businessInvite.IsUsed = true;
                }
            }
            await _unitOfWork.SaveChangesAsync();

            return userBusinessProfile;
        }

        public async Task<BusinessProfileToReturnDto?> CreateBusinessProfileOnly(AddBusinessProfileDto dto)
        {
            var businessProfile = _mapper.Map<BusinessProfile>(dto);
            if (businessProfile == null)
                throw new Exception(_localizationHelper.BusinessProfileIdInvalid);
            businessProfile.IsActive = true;
            await _unitOfWork.BusinessProfileRepository.AddAsync(businessProfile);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<BusinessProfileToReturnDto>(businessProfile);
        }

        public async Task<BusinessProfileToReturnDto?> CreateBusinessProfile(AddBusinessProfileDto dto, Guid ownerId)
        {
            var businessProfile = _mapper.Map<BusinessProfile>(dto);
            if (businessProfile == null)
                return null;

            await _unitOfWork.BusinessProfileRepository.AddAsync(businessProfile);
            await _unitOfWork.SaveChangesAsync();

            var userBusiness = new UserBusinessProfile
            {
                UserId = ownerId,
                BusinessProfileId = businessProfile.Id,
                Role = Shared.Enums.UserRole.Owner
            };
            await _unitOfWork.UserBusinessProfileRepository.AddAsync(userBusiness);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<BusinessProfileToReturnDto>(businessProfile);
        }

        public async Task<UserProfile?> CreateOwnerAsync(RegisterOwnerDto dto)
        {
            var existingUser = await _unitOfWork.UserProfileRepository
                .FindFirstOrDefaultAsync(u => u.Email == dto.User.Email);
            if (existingUser != null)
                return null;

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var newUser = _mapper.Map<UserProfile>(dto.User);
                newUser.PasswordHash = PasswordHelper.HashPassword(newUser.PasswordHash);
                await _unitOfWork.UserProfileRepository.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                var newBusiness = _mapper.Map<BusinessProfile>(dto.Business);
                await _unitOfWork.BusinessProfileRepository.AddAsync(newBusiness);
                await _unitOfWork.SaveChangesAsync();

                var userBusiness = new UserBusinessProfile
                {
                    UserId = newUser.Id,
                    BusinessProfileId = newBusiness.Id,
                    Role = Shared.Enums.UserRole.Owner
                };
                await _unitOfWork.UserBusinessProfileRepository.AddAsync(userBusiness);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                return newUser;
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }
        }

        public async Task<UserProfileToReturnDto?> CreateUserProfile(AddUserProfileDto userProfileDto)
        {
            var userProfile = _mapper.Map<UserProfile>(userProfileDto);
            if (userProfile == null)
                throw new Exception(_localizationHelper.UserProfileCreationFailed);

            userProfile.PasswordHash = PasswordHelper.HashPassword(userProfile.PasswordHash);

            await _unitOfWork.UserProfileRepository.AddAsync(userProfile);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserProfileToReturnDto>(userProfile);
        }

        public async Task<UserProfileToReturnDto?> GetByEmailAsync(string email)
        {
            var userProfile = await _unitOfWork.UserProfileRepository.FindFirstOrDefaultAsync(x => x.Email == email);
            return _mapper.Map<UserProfileToReturnDto>(userProfile);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.BeginTransactionAsync(cancellationToken);
        }

        public async Task SetEmailVerificationToken(Guid userId, string token, DateTime expiration)
        {
            var userProfile = await _unitOfWork.UserProfileRepository.GetByIdAsync(userId);
            if (userProfile == null)
                throw new Exception(_localizationHelper.UserNotFound);

            userProfile.EmailVerificationToken = token;
            userProfile.EmailVerificationTokenExpiration = expiration;

            _unitOfWork.UserProfileRepository.Update(userProfile);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> VerifyEmailToken(string email, string token)
        {
            var userProfile = await _unitOfWork.UserProfileRepository
                .FindFirstOrDefaultAsync(x => x.Email == email);

            if (userProfile == null)
                return false;

            if (userProfile.EmailVerificationToken != token)
                return false;

            if (userProfile.EmailVerificationTokenExpiration == null ||
                userProfile.EmailVerificationTokenExpiration < DateTime.UtcNow)
                return false;

            return true;
        }

        public async Task MarkEmailAsVerified(Guid userId)
        {
            var userProfile = await _unitOfWork.UserProfileRepository.GetByIdAsync(userId);
            if (userProfile == null)
                throw new Exception(_localizationHelper.UserNotFound);

            userProfile.IsEmailVerified = true;
            userProfile.EmailVerificationToken = null;
            userProfile.EmailVerificationTokenExpiration = null;

            _unitOfWork.UserProfileRepository.Update(userProfile);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<BusinessProfileToReturnDto?> GetCompanyByPibOrMb(string pib, string mb)
        {
            var company = await _unitOfWork.BusinessProfileRepository.FindFirstOrDefaultAsync(x =>(x.MB == mb) || (x.PIB == pib));
            return _mapper.Map<BusinessProfileToReturnDto>(company);
        }

        public async Task<BusinessInviteToReturnDto?> GetBusinessInvite(string email, string token)
        {
            var businessInvite = await _unitOfWork.BusinessInviteRepository.FindFirstOrDefaultAsync(x => x.Email == email && x.Token == token);
            return _mapper.Map<BusinessInviteToReturnDto>(businessInvite);
        }

        public async Task<BusinessProfileToReturnDto?> GetCompanyById(Guid id)
        {
            var business = await _unitOfWork.BusinessProfileRepository.FindFirstOrDefaultAsync(x => x.Id == id);
            return _mapper.Map<BusinessProfileToReturnDto>(business);
        }

       
    }
}