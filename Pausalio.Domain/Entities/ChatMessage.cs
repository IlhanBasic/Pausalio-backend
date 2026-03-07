using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Domain.Entities
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public Guid BusinessProfileId { get; set; }
        public BusinessProfile BusinessProfile { get; set; } = null!;

        public Guid SenderId { get; set; }
        public UserProfile Sender { get; set; } = null!;

        public Guid ReceiverId { get; set; }
        public UserProfile Receiver { get; set; } = null!;

        public string Content { get; set; } = null!;
        public MessageStatus Status { get; set; } = MessageStatus.Sent;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
