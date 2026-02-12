using AutoMapper;
using Pausalio.Application.DTOs.BusinessInvite;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Mappings
{
    public class BusinessInviteMappingProfile : Profile
    {
        public BusinessInviteMappingProfile()
        {
            CreateMap<BusinessInvite, BusinessInviteToReturnDto>()
                .ForMember(dest => dest.BusinessName,
                           opt => opt.MapFrom(src => src.BusinessProfile.BusinessName))
                .ForMember(dest => dest.CreatedBy,
                           opt => opt.MapFrom(src => src.CreatedBy.Email));
            CreateMap<AddBusinessInviteDto, BusinessInvite>();
            CreateMap<UpdateBusinessInviteDto, BusinessInvite>();
        }
    }
}
