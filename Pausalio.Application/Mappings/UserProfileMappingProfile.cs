using AutoMapper;
using Pausalio.Application.DTOs.UserProfile;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            CreateMap<UpdateUserProfileDto, UserProfile>();
            CreateMap<UserProfile, UserProfileToReturnDto>();
        }
    }
}
