using Microsoft.Extensions.Logging;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Implementations.AIAssistant
{
    public class ToolCallResult
    {
        public required object ToolMessage { get; init; }
        public required string ToolResult { get; init; }
    }

    public class ToolCallProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AIAssistantToolExecutor _toolExecutor;
        private readonly ILogger _logger;

        public ToolCallProcessor(
            IUnitOfWork unitOfWork,
            AIAssistantToolExecutor toolExecutor,
            ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _toolExecutor = toolExecutor;
            _logger = logger;
        }

        public async Task<ToolCallResult> ProcessToolCallAsync(
            string rawToolCall,
            Guid userAiMessageId,
            int toolCallRound,
            CachedToolData cachedData)
        {
            string toolResult;
            var toolCallId = "unknown";
            var functionName = "unknown";
            var argumentsJson = "{}";
            var isSuccess = true;
            string? errorMessage = null;

            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var toolDoc = JsonDocument.Parse(rawToolCall);
                var toolCall = toolDoc.RootElement;
                toolCallId = toolCall.TryGetProperty("id", out var idProp)
                    ? idProp.GetString() ?? "unknown"
                    : "unknown";

                var functionElement = toolCall.GetProperty("function");
                functionName = functionElement.GetProperty("name").GetString()!;
                var argumentsElement = functionElement.GetProperty("arguments");
                argumentsJson = argumentsElement.ValueKind == JsonValueKind.String
                    ? argumentsElement.GetString()!
                    : argumentsElement.GetRawText();

                toolResult = _toolExecutor.ExecuteTool(functionName, argumentsJson, cachedData);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse or execute tool call {ToolCallId}", toolCallId);
                toolResult = $"Invalid tool call or arguments: {ex.Message}";
                isSuccess = false;
                errorMessage = ex.Message;
            }

            stopwatch.Stop();

            var aiToolCall = new AiToolCall
            {
                Id = Guid.NewGuid(),
                MessageId = userAiMessageId,
                ToolName = functionName,
                Arguments = argumentsJson,
                Result = toolResult,
                Success = isSuccess,
                ErrorMessage = errorMessage,
                RoundNumber = toolCallRound,
                DurationMs = (int)stopwatch.ElapsedMilliseconds,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.AiToolCallRepository.AddAsync(aiToolCall);

            var toolMessage = new
            {
                role = "tool",
                tool_call_id = toolCallId,
                content = toolResult
            };

            return new ToolCallResult
            {
                ToolMessage = toolMessage,
                ToolResult = toolResult
            };
        }
    }
}
