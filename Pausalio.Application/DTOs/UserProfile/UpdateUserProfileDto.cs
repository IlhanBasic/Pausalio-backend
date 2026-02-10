using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.UserProfile
{
    public class UpdateUserProfileDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public string? Phone { get; set; }
        public string City { get; set; } = string.Empty;
        public string? Address { get; set; }
    }
}
