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
            CreateMap<AddUserProfileDto, UserProfile>();
            CreateMap<UpdateUserProfileDto, UserProfile>();
            CreateMap<UserProfile, UserProfileToReturnDto>();
        }
    }
}
