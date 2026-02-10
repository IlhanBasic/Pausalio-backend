using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Domain.Entities
{
    public class Item
    {
        public Guid Id { get; set; }
        public Guid BusinessProfileId { get; set; }
        public BusinessProfile BusinessProfile { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; } = null;
        public ItemType ItemType { get; set; } = ItemType.Product;
        public decimal UnitPrice { get; set; }
    }
}
