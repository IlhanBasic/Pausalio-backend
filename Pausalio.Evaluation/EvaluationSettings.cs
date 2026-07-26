using System;

namespace Pausalio.Evaluation
{
    public class EvaluationSettings
    {
        public string GenerationModel { get; set; } = "openai/gpt-4o-mini";
        public string JudgeModel { get; set; } = "meta-llama/llama-3.3-70b-instruct:free";
        public string OpenRouterApiKey { get; set; } = string.Empty;
        public string OpenRouterApiUrl { get; set; } = "https://openrouter.ai/api/v1/chat/completions";
        public int DelayBetweenQuestionsMs { get; set; } = 3500;
        public int MaxRetries { get; set; } = 3;
    }
}
