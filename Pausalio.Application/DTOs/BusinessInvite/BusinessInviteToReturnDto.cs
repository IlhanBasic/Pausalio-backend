using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.BusinessInvite
{
    public class BusinessInviteToReturnDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid BusinessProfileId { get; set; }
        public Guid CreatedById { get; set; }
        public string? CreatedBy {  get; set; }
        public string? BusinessName { get; set; }
    }
}
