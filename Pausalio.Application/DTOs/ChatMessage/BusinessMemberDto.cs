using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.ChatMessage
{
    public class BusinessMemberDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public string Role { get; set; } = null!;
    }
}
