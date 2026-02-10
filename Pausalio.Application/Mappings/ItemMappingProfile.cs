using AutoMapper;
using Pausalio.Application.DTOs.Item;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Mappings
{
    public class ItemMappingProfile : Profile
    {
        public ItemMappingProfile()
        {
            CreateMap<Item, ItemToReturnDto>();
            CreateMap<AddItemDto, Item>();
            CreateMap<UpdateItemDto, Item>();
        }
    }
}
