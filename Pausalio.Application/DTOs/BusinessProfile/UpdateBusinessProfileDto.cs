using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.BusinessProfile
{
    public class UpdateBusinessProfileDto
    {
        public string BusinessName { get; set; } = null!;
        public string PIB { get; set; } = null!;
        public string MB { get; set; } = string.Empty;
        public Guid ActivityCodeId { get; set; }
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? CompanyLogo { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
