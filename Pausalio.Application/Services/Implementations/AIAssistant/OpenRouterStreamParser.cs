using Pausalio.Application.DTOs.AIAssistant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Implementations.AIAssistant
{
    public class OpenRouterStreamParser
    {
        private class ToolCallAccumulator
        {
            public int Index { get; set; }
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public StringBuilder Arguments { get; } = new StringBuilder();
        }

        public async Task<OpenRouterResponse> ParseStreamAsync(
            Stream stream,
            StringBuilder assistantBuffer,
            Guid conversationId,
            Func<AiStreamChunkDto, Task> onChunk,
            CancellationToken cancellationToken)
        {
            var finishReason = string.Empty;
            AIUsageDto? usage = null;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var activeToolCalls = new Dictionary<int, ToolCallAccumulator>();

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (line == null)
                    break;

                line = line.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith(":"))
                    continue;

                if (line.StartsWith("data:"))
                    line = line.Substring("data:".Length).Trim();

                if (string.IsNullOrEmpty(line))
                    continue;

                if (line == "[DONE]")
                    break;

                try
                {
                    using var document = JsonDocument.Parse(line);
                    var root = document.RootElement;

                    if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                    {
                        var choice = choices[0];

                        if (choice.TryGetProperty("delta", out var delta))
                        {
                            if (delta.TryGetProperty("content", out var contentElement)
                                && contentElement.ValueKind == JsonValueKind.String)
                            {
                                var content = contentElement.GetString();
                                if (!string.IsNullOrEmpty(content))
                                {
                                    assistantBuffer.Append(content);
                                    await onChunk(new AiStreamChunkDto
                                    {
                                        ConversationId = conversationId,
                                        Type = "content",
                                        Content = content,
                                        IsFinal = false
                                    });
                                }
                            }

                            if (delta.TryGetProperty("tool_calls", out var toolCallsElement)
                                && toolCallsElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var element in toolCallsElement.EnumerateArray())
                                {
                                    int index = 0;
                                    if (element.TryGetProperty("index", out var indexProp) && indexProp.TryGetInt32(out var parsedIdx))
                                    {
                                        index = parsedIdx;
                                    }

                                    if (!activeToolCalls.TryGetValue(index, out var tc))
                                    {
                                        tc = new ToolCallAccumulator { Index = index };
                                        activeToolCalls[index] = tc;
                                    }

                                    if (element.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                                    {
                                        tc.Id = idProp.GetString() ?? tc.Id;
                                    }

                                    if (element.TryGetProperty("function", out var funcProp) && funcProp.ValueKind == JsonValueKind.Object)
                                    {
                                        if (funcProp.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                                        {
                                            tc.Name = nameProp.GetString() ?? tc.Name;
                                        }

                                        if (funcProp.TryGetProperty("arguments", out var argsProp) && argsProp.ValueKind == JsonValueKind.String)
                                        {
                                            tc.Arguments.Append(argsProp.GetString());
                                        }
                                    }
                                }
                            }
                        }

                        if (choice.TryGetProperty("finish_reason", out var finishReasonElement)
                            && finishReasonElement.ValueKind == JsonValueKind.String)
                        {
                            finishReason = finishReasonElement.GetString() ?? finishReason;
                        }
                    }

                    if (usage == null && root.TryGetProperty("usage", out var usageElement))
                    {
                        usage = ParseUsageFromJsonElement(usageElement);
                    }
                }
                catch (JsonException)
                {
                    // Ignore invalid JSON lines such as non-JSON events or keep-alive markers.
                }
            }

            object? assistantMessageObject = null;
            var toolCallRawMessages = new List<string>();

            if (activeToolCalls.Count > 0)
            {
                var sortedToolCalls = activeToolCalls.Values.OrderBy(x => x.Index).ToList();

                toolCallRawMessages = sortedToolCalls.Select(tc => JsonSerializer.Serialize(new
                {
                    id = tc.Id,
                    type = "function",
                    function = new
                    {
                        name = tc.Name,
                        arguments = tc.Arguments.ToString()
                    }
                })).ToList();

                assistantMessageObject = new
                {
                    role = "assistant",
                    content = assistantBuffer.Length > 0 ? assistantBuffer.ToString() : null,
                    tool_calls = sortedToolCalls.Select(tc => new
                    {
                        id = tc.Id,
                        type = "function",
                        function = new
                        {
                            name = tc.Name,
                            arguments = tc.Arguments.ToString()
                        }
                    }).ToArray()
                };
            }

            return new OpenRouterResponse
            {
                FinishReason = string.IsNullOrEmpty(finishReason) ? (activeToolCalls.Count > 0 ? "tool_calls" : "unknown") : finishReason,
                AssistantMessage = assistantBuffer.ToString(),
                MessageRaw = assistantBuffer.ToString(),
                AssistantMessageObject = assistantMessageObject,
                ToolCallRawMessages = toolCallRawMessages,
                Usage = usage
            };
        }

        private static AIUsageDto? ParseUsageFromJsonElement(JsonElement usageElement)
        {
            if (usageElement.ValueKind != JsonValueKind.Object)
                return null;

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
                ConversationId = Guid.Empty,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = totalTokens
            };
        }
    }
}
