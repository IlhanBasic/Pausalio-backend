using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Application.DTOs.UserProfile;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.UserBusinessProfile
{
    public class UserBusinessProfileToReturnDto
    {
        public Guid Id { get; set; }
        public UserProfileToReturnDto User { get; set; } = null!;
        public BusinessProfileToReturnDto BusinessProfile { get; set; } = null!;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
