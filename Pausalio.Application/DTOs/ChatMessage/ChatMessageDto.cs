using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.ChatMessage
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = null!;
        public string? SenderAvatar { get; set; }
        public Guid ReceiverId { get; set; }
        public string ReceiverName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int Status { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
