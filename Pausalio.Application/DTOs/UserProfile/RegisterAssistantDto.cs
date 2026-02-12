using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.UserProfile
{
    public class RegisterAssistantDto
    {
        public string InviteToken { get; set; } = null!;
        public AddUserProfileDto User { get; set; } = null!;
    }
}
