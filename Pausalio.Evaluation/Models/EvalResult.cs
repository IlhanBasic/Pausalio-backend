using System;
using System.Collections.Generic;

namespace Pausalio.Evaluation.Models
{
    public class ToolCallInfo
    {
        public string ToolName { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int RoundNumber { get; set; }
        public long DurationMs { get; set; }
    }

    public class EvalResult
    {
        public int QuestionId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public string AssistantResponse { get; set; } = string.Empty;
        public List<string> ExpectedTools { get; set; } = new();
        public Dictionary<string, object> ExpectedParameters { get; set; } = new();
        
        public List<ToolCallInfo> ActualToolCalls { get; set; } = new();
        
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens => PromptTokens + CompletionTokens;
        
        public string ConversationId { get; set; } = string.Empty;
        public double ExecutionDurationMs { get; set; }
        
        // Level 1: Tool Selection Match
        public bool IsToolCallMatch { get; set; }
        
        // Level 2: Parameter Accuracy
        public double ParameterAccuracyScore { get; set; }
        
        // Level 3: LLM Judge scoring
        public string JudgeReasoning { get; set; } = string.Empty;
        public int JudgeAccuracy { get; set; } // 0-5
        public int JudgeCompleteness { get; set; } // 0-2
        public int JudgeClarity { get; set; } // 0-3
        public bool Ignored { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
