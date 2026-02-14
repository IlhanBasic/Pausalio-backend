using Pausalio.Application.DTOs.BusinessProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.UserProfile
{
        public class RegisterOwnerDto
        {
            public AddUserProfileDto User { get; set; } = null!;
            public AddBusinessProfileDto Business { get; set; } = null!;
        }
}
