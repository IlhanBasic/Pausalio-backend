using Microsoft.EntityFrameworkCore.Storage;
using Pausalio.Application.DTOs.BusinessInvite;
using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Application.DTOs.UserProfile;
using Pausalio.Domain.Entities;
using Pausalio.Shared.Enums;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileToReturnDto?> LoginAsync(string email, string password);
        Task<UserProfileToReturnDto?> CreateUserProfile(AddUserProfileDto userProfile, UserRole role);
        Task<UserProfileToReturnDto?> GetByEmailAsync(string email);
        Task<BusinessInviteToReturnDto?> GetBusinessInvite(string email, string token);
        Task<BusinessProfileToReturnDto?> GetCompanyByPibOrMb(string pib, string mb);
        Task<BusinessProfileToReturnDto?> GetCompanyById(Guid id);
        Task<BusinessProfileToReturnDto?> CreateBusinessProfile(AddBusinessProfileDto dto, Guid ownerId);
        Task<BusinessProfileToReturnDto?> CreateBusinessProfileOnly(AddBusinessProfileDto dto);
        Task<UserProfile?> CreateOwnerAsync(RegisterOwnerDto dto);
        Task<UserBusinessProfile> AddUserToBusinessProfile(Guid userId, Guid businessProfileId, UserBusinessRole role);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task SetEmailVerificationToken(Guid userId, string token, DateTime expiration);
        Task<bool> VerifyEmailToken(string email, string token);
        Task MarkEmailAsVerified(Guid userId);
        Task<bool> IsUserInBusiness(Guid userId, Guid businessProfileId);
        Task DeleteBusinessInvite(Guid inviteId);
        Task<bool> IsUserOwnerInAnyBusiness(Guid userId);
        Task ChangePassword(ChangePasswordDto changePasswordDto);
        Task SetPasswordResetTokenAsync(string email, string token, DateTime expiration);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task<UserProfileToReturnDto?> UpdateProfile(Guid id, UpdateUserProfileDto dto);
        Task<IEnumerable<UserProfileToReturnDto>> GetAllUsers();
        Task SetUserActiveStatus (Guid userId, bool isActive);
        Task DeleteUser(Guid userId);
    }
}
