using System;

namespace Pausalio.Application.DTOs.AIAssistant
{
    public class AiMessageDto
    {
        public Guid Id { get; set; }
        public string Role { get; set; } = null!;
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}