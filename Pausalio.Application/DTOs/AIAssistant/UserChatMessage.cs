using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.DTOs.AIAssistant
{
    public class UserChatMessage
    {
        public Guid? ConversationId { get; set; }
        public string Message { get; set; } = null!;
        public List<ChatHistoryItem> History { get; set; } = new();
    }

    public class ChatHistoryItem
    {
        public string Role { get; set; } = null!;
        public string Content { get; set; } = null!;
    }

    public class AIResponseDto
    {
        public Guid ConversationId { get; set; }
        public string Message { get; set; } = null!;
        public AIUsageDto? Usage { get; set; }
    }

    public class AIUsageDto
    {
        public Guid ConversationId { get; set; }
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}
