using AutoMapper;
using Pausalio.Application.DTOs.Invoice;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Mappings
{
    public class InvoiceMappingProfile : Profile
    {
        public InvoiceMappingProfile()
        {
            CreateMap<AddInvoiceDto, Invoice>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
            CreateMap<UpdateInvoiceDto, Invoice>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Invoice, InvoiceToReturnDto>();
        }
    }
}
