using AutoMapper;
using Pausalio.Application.DTOs.KpoBook;
using Pausalio.Domain.Entities;

namespace Pausalio.Application.Mappings
{
    public class KpoEntryMappingProfile : Profile
    {
        public KpoEntryMappingProfile()
        {
            CreateMap<KpoEntry, KpoEntryToReturnDto>()
                .ForMember(dest => dest.InvoiceNumber,
                    opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.InvoiceNumber : null));
        }
    }
}
