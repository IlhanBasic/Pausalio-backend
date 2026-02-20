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
            CreateMap<UpdateBusinessProfileDto, BusinessProfile>()
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UserBusinessProfiles, opt => opt.Ignore())
                .ForMember(dest => dest.BankAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.Clients, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore())
                .ForMember(dest => dest.Expenses, opt => opt.Ignore())
                .ForMember(dest => dest.TaxObligations, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.Reminders, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore())
                .ForMember(dest => dest.BusinessInvites, opt => opt.Ignore())
                .ForMember(dest => dest.ActivityCode, opt => opt.Ignore());
        }
    }
}
