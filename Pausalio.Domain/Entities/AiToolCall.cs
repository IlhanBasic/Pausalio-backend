using System;

namespace Pausalio.Domain.Entities
{
    public class AiToolCall
    {
        public Guid Id { get; set; }
        public Guid MessageId { get; set; }
        public string ToolName { get; set; } = null!;
        public string Arguments { get; set; } = null!;
        public string? Result { get; set; }
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public int RoundNumber { get; set; }
        public int? DurationMs { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public AiMessage Message { get; set; } = null!;
    }
}