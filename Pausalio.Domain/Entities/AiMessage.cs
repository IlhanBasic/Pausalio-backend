using System;
using System.Collections.Generic;

namespace Pausalio.Domain.Entities
{
    public class AiMessage
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public string Role { get; set; } = null!;
        public string? Content { get; set; }
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public AiConversation Conversation { get; set; } = null!;
        public ICollection<AiToolCall> ToolCalls { get; set; } = new List<AiToolCall>();
    }
}