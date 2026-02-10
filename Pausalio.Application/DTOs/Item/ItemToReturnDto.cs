using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.Item
{
    public class ItemToReturnDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; } = null;
        public ItemType ItemType { get; set; } = ItemType.Product;
        public decimal UnitPrice { get; set; }
    }
}
