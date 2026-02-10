using AutoMapper;
using Pausalio.Application.DTOs.InvoiceItem;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Mappings
{
    public class InvoiceItemMappingProfile : Profile
    {
        public InvoiceItemMappingProfile()
        {
            CreateMap<AddInvoiceItemDto, InvoiceItem>();
            CreateMap<UpdateInvoiceItemDto, InvoiceItem>();
            CreateMap<InvoiceItem, InvoiceItemToReturnDto>();
        }
    }
}
