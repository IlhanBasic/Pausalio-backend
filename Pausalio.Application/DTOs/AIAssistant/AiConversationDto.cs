using System;

namespace Pausalio.Application.DTOs.AIAssistant
{
    public class AiConversationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}