using AutoMapper;
using Pausalio.Application.DTOs.BankAccount;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Mappings
{
    public class BankAccountMappingProfile : Profile
    {
        public BankAccountMappingProfile()
        {
            CreateMap<BankAccount, BankAccountToReturnDto>();
            CreateMap<AddBankAccountDto, BankAccount>();
            CreateMap<UpdateBankAccountDto, BankAccount>();
        }
    }
}
