using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Application.DTOs.Invoice;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Client
{
    public class ClientToReturnDto
    {
        public Guid Id { get; set; }
        public ClientType ClientType { get; set; } = ClientType.Domestic;
        public string Name { get; set; } = null!;
        public string? PIB { get; set; } = null;
        public string? MB { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Country { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        //public ICollection<InvoiceToReturnDto> Invoices { get; set; } = new List<InvoiceToReturnDto>();
    }
}
