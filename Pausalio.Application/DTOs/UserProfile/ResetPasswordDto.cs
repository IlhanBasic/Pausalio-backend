using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.UserProfile
{
    public class ResetPasswordDto
    {
        public string Email { get; set; } = null!;
        public string Pin { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
