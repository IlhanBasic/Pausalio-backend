using AutoMapper;
using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Mappings
{
    public class BusinessProfileMappingProfile : Profile
    {
        public BusinessProfileMappingProfile()
        {
            CreateMap<BusinessProfile, BusinessProfileToReturnDto>()
                .ForMember(
                    dest => dest.ActivityCode,
                    opt => opt.MapFrom(src => src.ActivityCode.Description)
                );
            CreateMap<AddBusinessProfileDto, BusinessProfile>();
            CreateMap<UpdateBusinessProfileDto, BusinessProfile>();
        }
    }
}
