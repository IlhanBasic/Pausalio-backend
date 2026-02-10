using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Payment
{
    public class UpdatePaymentDto
    {
        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }
    }
}
