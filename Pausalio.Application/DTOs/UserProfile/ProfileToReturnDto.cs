using Pausalio.Application.DTOs.BusinessProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.UserProfile
{
    public class ProfileToReturnDto
    {
        public UserProfileToReturnDto UserProfile { get; set; } = null!;
        public BusinessProfileToReturnDto? BusinessProfile { get; set; }
    }
}
