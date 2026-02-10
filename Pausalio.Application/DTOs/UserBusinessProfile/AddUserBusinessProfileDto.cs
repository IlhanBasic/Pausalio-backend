using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.UserBusinessProfile
{
    public class AddUserBusinessProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BusinessProfileId { get; set; }
        public UserRole Role { get; set; }
    }
}
