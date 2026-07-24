using Pausalio.Application.DTOs.AIAssistant;
using System.Text.Json;

namespace Pausalio.Application.Services.Implementations.AIAssistant
{
    public class OpenRouterResponseParser
    {
        public OpenRouterResponse Parse(string responseJson)
        {
            using var document = JsonDocument.Parse(responseJson);
            var root = document.RootElement;

            var choice = root.GetProperty("choices")[0];
            var finishReason = choice.GetProperty("finish_reason").GetString() ?? string.Empty;
            var messageElement = choice.GetProperty("message");
            var assistantMessage = messageElement.GetProperty("content").GetString();
            var rawMessage = messageElement.GetRawText();

            var toolCalls = new List<string>();
            if (messageElement.TryGetProperty("tool_calls", out var toolCallsElement)
                && toolCallsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var toolCall in toolCallsElement.EnumerateArray())
                {
                    toolCalls.Add(toolCall.GetRawText());
                }
            }

            return new OpenRouterResponse
            {
                FinishReason = finishReason,
                AssistantMessage = assistantMessage,
                AssistantMessageObject = JsonSerializer.Deserialize<object>(rawMessage),
                MessageRaw = rawMessage,
                ToolCallRawMessages = toolCalls,
                Usage = ParseUsage(root)
            };
        }

        private static AIUsageDto? ParseUsage(JsonElement root)
        {
            if (!root.TryGetProperty("usage", out var usageElement)
                || usageElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var promptTokens = usageElement.TryGetProperty("prompt_tokens", out var promptElement) && promptElement.TryGetInt32(out var promptValue)
                ? promptValue
                : 0;
            var completionTokens = usageElement.TryGetProperty("completion_tokens", out var completionElement) && completionElement.TryGetInt32(out var completionValue)
                ? completionValue
                : 0;
            var totalTokens = usageElement.TryGetProperty("total_tokens", out var totalElement) && totalElement.TryGetInt32(out var totalValue)
                ? totalValue
                : 0;

            return new AIUsageDto
            {
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = totalTokens
            };
        }
    }

    public class OpenRouterResponse
    {
        public required string FinishReason { get; init; }
        public string? AssistantMessage { get; init; }
        public object? AssistantMessageObject { get; init; }
        public required string MessageRaw { get; init; }
        public required IReadOnlyList<string> ToolCallRawMessages { get; init; }
        public AIUsageDto? Usage { get; init; }
    }
}
