using System;

namespace Pausalio.Application.DTOs.AIAssistant
{
    public class AiStreamChunkDto
    {
        public Guid ConversationId { get; set; }
        public string Type { get; set; } = null!;
        public string Content { get; set; } = string.Empty;
        public bool IsFinal { get; set; }
        public AIUsageDto? Usage { get; set; }
    }
}
