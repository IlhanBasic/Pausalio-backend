using AutoMapper;
using Pausalio.Application.DTOs.Payment;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Mappings
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            CreateMap<AddPaymentDto, Payment>();
            CreateMap<UpdatePaymentDto, Payment>();
            CreateMap<Payment, PaymentToReturnDto>();
        }
    }
}
