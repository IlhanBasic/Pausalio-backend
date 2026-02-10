using AutoMapper;
using Pausalio.Application.DTOs.Reminder;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Mappings
{
    public class ReminderMappingProfile : Profile
    {
        public ReminderMappingProfile()
        {
            CreateMap<Reminder, ReminderToReturnDto>();
            CreateMap<AddReminderDto, Reminder>();
            CreateMap<UpdateReminderDto, Reminder>();
        }
    }
}
