using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Domain.Entities
{
    public class UserBusinessProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserProfile User { get; set; } = null!;
        public Guid BusinessProfileId { get; set; }
        public BusinessProfile BusinessProfile { get; set; } = null!;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
