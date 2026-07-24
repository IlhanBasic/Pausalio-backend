using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace Pausalio.Domain.Entities
{
    public class AiConversation
    {
        public Guid Id { get; set; }
        public Guid BusinessProfileId { get; set; }
        public Guid UserId { get; set; }
        public string? Title { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public BusinessProfile BusinessProfile { get; set; } = null!;
        public UserProfile User { get; set; } = null!;
        public ICollection<AiMessage> Messages { get; set; } = new List<AiMessage>();
    }
}