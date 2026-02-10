using AutoMapper;
using Pausalio.Application.DTOs.UserBusinessProfile;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Mappings
{
    public class UserBusinessProfileMappingProfile : Profile
    {
        public UserBusinessProfileMappingProfile()
        {
            CreateMap<AddUserBusinessProfileDto, UserBusinessProfile>();
            CreateMap<UpdateUserBusinessProfileDto, UserBusinessProfile>();
            CreateMap<UserBusinessProfile, UserBusinessProfileToReturnDto>();
        }
    }
}
