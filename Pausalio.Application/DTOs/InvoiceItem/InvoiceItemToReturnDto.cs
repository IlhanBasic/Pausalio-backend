using Pausalio.Application.DTOs.Invoice;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.InvoiceItem
{
    public class InvoiceItemToReturnDto
    {
        public Guid Id { get; set; }
        //public InvoiceToReturnDto Invoice { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ItemType ItemType { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
