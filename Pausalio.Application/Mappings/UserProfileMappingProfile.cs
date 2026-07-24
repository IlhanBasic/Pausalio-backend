using AutoMapper;
using Pausalio.Application.DTOs.UserProfile;
using Pausalio.Domain.Entities;

namespace Pausalio.Application.Mappings
{
    public class UserProfileMappingProfile : Profile
    {
        public UserProfileMappingProfile()
        {
            CreateMap<AddUserProfileDto, UserProfile>()
                .ForMember(
                    dest => dest.PasswordHash,
                    opt => opt.MapFrom(src => src.Password)
                );

            CreateMap<UpdateUserProfileDto, UserProfile>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.OpenRouterApiKey, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.IsEmailVerified, opt => opt.Ignore())
                .ForMember(dest => dest.EmailVerificationToken, opt => opt.Ignore())
                .ForMember(dest => dest.EmailVerificationTokenExpiration, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordResetToken, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordResetTokenExpiration, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UserBusinessProfiles, opt => opt.Ignore());

            CreateMap<UserProfile, UserProfileToReturnDto>()
                .ForMember(
                    dest => dest.IsOpenRouterAPIKeySet,
                    opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.OpenRouterApiKey))
                )
                .ForMember(
                    dest => dest.IsOpenRouterModelSet,
                    opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.OpenRouterModelName))
                );
        }
    }
}