using AutoMapper;
using Pausalio.Application.DTOs.Client;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Mappings
{
    public class ClientMappingProfile : Profile
    {
        public ClientMappingProfile()
        {
            CreateMap<Client, ClientToReturnDto>()
                .ForMember(
                    dest => dest.Country,
                    opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : null)
                );
            CreateMap<AddClientDto, Client>();
            CreateMap<UpdateClientDto, Client>();
        }
    }
}
