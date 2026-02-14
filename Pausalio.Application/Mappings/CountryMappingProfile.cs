using AutoMapper;
using Pausalio.Application.DTOs.Country;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Mappings
{
    public class CountryMappingProfile : Profile
    {
        public CountryMappingProfile()
        {
            CreateMap<Country, CountryToReturnDto>();
            CreateMap<AddCountryDto, Country>();
            CreateMap<UpdateCountyDto, Country>();
        }
    }
}
