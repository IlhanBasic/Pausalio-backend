using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Client
{
    public class AddClientDto
    {
        public ClientType ClientType { get; set; } = ClientType.Domestic;
        public string Name { get; set; } = null!;
        public string? PIB { get; set; } = null;
        public string? MB { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public Guid? CountryId { get; set; }
    }
}
