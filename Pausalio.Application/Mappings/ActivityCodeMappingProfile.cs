using AutoMapper;
using Pausalio.Application.DTOs.ActivityCode;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Mappings
{
    public class ActivityCodeMappingProfile : Profile
    {
        public ActivityCodeMappingProfile()
        {
            CreateMap<ActivityCode, ActivityCodeToReturnDto>();
        }
    }
}
