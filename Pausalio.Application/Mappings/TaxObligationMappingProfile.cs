using AutoMapper;
using Pausalio.Application.DTOs.TaxObligation;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Mappings
{
    public class TaxObligationMappingProfile : Profile
    {
        public TaxObligationMappingProfile()
        {
            CreateMap<AddTaxObligationDto, TaxObligation>();
            CreateMap<UpdateTaxObligationDto, TaxObligation>();
            CreateMap<TaxObligation, TaxObligationToReturnDto>();
        }
    }
}
