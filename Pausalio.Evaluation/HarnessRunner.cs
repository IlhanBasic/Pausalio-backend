using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pausalio.Application.DTOs.AIAssistant;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Evaluation.Models;
using Pausalio.Infrastructure.Repositories.Interfaces;

namespace Pausalio.Evaluation
{
    /// <summary>
    /// Retry strategy chosen: new conversation per retry (less invasive — does not modify
    /// IAIAssistantService's public contract). Orphan conversation IDs from failed attempts
    /// are logged as warnings so token waste can be manually inspected.
    /// Only the final successful conversation's tokens and tool calls are attributed.
    /// </summary>
    public class HarnessRunner
    {
        private readonly IAIAssistantService _aiAssistantService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly EvaluationSettings _settings;
        private readonly ILogger<HarnessRunner> _logger;
        private const string ResultsFilePath = "results.jsonl";

        public HarnessRunner(
            IAIAssistantService aiAssistantService,
            IUnitOfWork unitOfWork,
            IOptions<EvaluationSettings> settings,
            ILogger<HarnessRunner> logger)
        {
            _aiAssistantService = aiAssistantService;
            _unitOfWork = unitOfWork;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<List<EvalResult>> RunAsync(List<EvalQuestion> questions)
        {
            var skippedIds = LoadExistingQuestionIds();
            _logger.LogInformation("Loaded {Count} already processed questions. Resuming evaluation...", skippedIds.Count);

            var results = new List<EvalResult>();

            foreach (var q in questions)
            {
                if (skippedIds.Contains(q.Id))
                {
                    _logger.LogInformation("Skipping question ID {Id} (already in JSONL)", q.Id);
                    continue;
                }

                _logger.LogInformation("Processing question ID {Id}: \"{Question}\"", q.Id, q.Question);

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var result = new EvalResult
                {
                    QuestionId = q.Id,
                    Category = q.Category,
                    Question = q.Question,
                    ExpectedTools = q.ExpectedTools,
                    ExpectedParameters = q.ExpectedParameters
                };

                try
                {
                    AIResponseDto? responseDto = null;
                    int retries = 0;
                    int delayMs = 2000;
                    var orphanConversationIds = new List<Guid>();

                    // Snapshot conversation IDs before first attempt so we can diff later
                    var evalUserId = EvalCurrentUserService.SeededUserId;
                    var preExistingConversationIds = await GetConversationIdsForUser(evalUserId);

                    while (true)
                    {
                        try
                        {
                            var chatMessage = new UserChatMessage
                            {
                                ConversationId = Guid.Empty,  // always new conversation per attempt
                                Message = q.Question,
                                History = new List<ChatHistoryItem>()
                            };

                            responseDto = await _aiAssistantService.SendMessageAsync(chatMessage);
                            break;
                        }
                        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests && retries < _settings.MaxRetries)
                        {
                            retries++;
                            _logger.LogWarning("HTTP 429 received for question {Id}. Retry {Attempt}/{MaxRetries} in {Delay}ms.",
                                q.Id, retries, _settings.MaxRetries, delayMs);
                            await Task.Delay(delayMs);
                            delayMs *= 2;
                        }
                    }

                    stopwatch.Stop();

                    // Detect orphan conversations from failed retries
                    if (retries > 0)
                    {
                        var postConversationIds = await GetConversationIdsForUser(evalUserId);
                        var newConversations = postConversationIds.Except(preExistingConversationIds).ToList();
                        orphanConversationIds = newConversations.Where(id => id != responseDto.ConversationId).ToList();

                        if (orphanConversationIds.Any())
                        {
                            _logger.LogWarning(
                                "Question {Id}: {Count} orphan conversation(s) from failed retries: [{OrphanIds}]. " +
                                "These represent wasted tokens — inspect manually if needed.",
                                q.Id,
                                orphanConversationIds.Count,
                                string.Join(", ", orphanConversationIds));
                        }
                    }

                    result.ConversationId = responseDto.ConversationId.ToString();
                    result.AssistantResponse = responseDto.Message;
                    result.ExecutionDurationMs = stopwatch.ElapsedMilliseconds;

                    // Query DB for tool calls and messages under the FINAL (successful) conversation only
                    var context = _unitOfWork.GetContext();
                    var dbToolCalls = await context.Set<AiToolCall>()
                        .Include(tc => tc.Message)
                        .Where(tc => tc.Message.ConversationId == responseDto.ConversationId)
                        .OrderBy(tc => tc.CreatedAt)
                        .ToListAsync();

                    var dbMessages = await context.Set<AiMessage>()
                        .Where(m => m.ConversationId == responseDto.ConversationId)
                        .ToListAsync();

                    result.ActualToolCalls = dbToolCalls.Select(tc => new ToolCallInfo
                    {
                        ToolName = tc.ToolName,
                        Arguments = tc.Arguments,
                        Result = tc.Result ?? string.Empty,
                        Success = tc.Success,
                        RoundNumber = tc.RoundNumber,
                        DurationMs = tc.DurationMs ?? 0
                    }).ToList();

                    // Token accounting: prefer DTO (already accumulated across rounds),
                    // fall back to DB message-level sums
                    int promptTokens = 0, completionTokens = 0;
                    if (responseDto.Usage != null && responseDto.Usage.TotalTokens > 0)
                    {
                        promptTokens = responseDto.Usage.PromptTokens;
                        completionTokens = responseDto.Usage.CompletionTokens;
                    }
                    else
                    {
                        promptTokens = dbMessages.Sum(m => m.PromptTokens ?? 0);
                        completionTokens = dbMessages.Sum(m => m.CompletionTokens ?? 0);
                    }

                    result.PromptTokens = promptTokens;
                    result.CompletionTokens = completionTokens;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "Error processing question ID {Id}", q.Id);
                    result.ErrorMessage = ex.Message;
                    result.Ignored = true;
                    result.ExecutionDurationMs = stopwatch.ElapsedMilliseconds;
                }

                // Calculate metrics Level 1 & 2
                MetricsCalculator.Calculate(result);

                // Append to JSONL immediately for resumability
                AppendToJsonl(result);

                results.Add(result);

                // Add configurable delay between questions
                if (_settings.DelayBetweenQuestionsMs > 0)
                {
                    await Task.Delay(_settings.DelayBetweenQuestionsMs);
                }
            }

            return results;
        }

        private async Task<HashSet<Guid>> GetConversationIdsForUser(Guid userId)
        {
            var context = _unitOfWork.GetContext();
            var ids = await context.Set<AiConversation>()
                .Where(c => c.UserId == userId)
                .Select(c => c.Id)
                .ToListAsync();
            return ids.ToHashSet();
        }

        private HashSet<int> LoadExistingQuestionIds()
        {
            var ids = new HashSet<int>();
            if (!File.Exists(ResultsFilePath))
            {
                return ids;
            }

            foreach (var line in File.ReadLines(ResultsFilePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    if (doc.RootElement.TryGetProperty("QuestionId", out var idProp))
                    {
                        ids.Add(idProp.GetInt32());
                    }
                }
                catch
                {
                    // Ignore malformed lines on resume check
                }
            }

            return ids;
        }

        private void AppendToJsonl(EvalResult result)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false
            };
            var json = JsonSerializer.Serialize(result, options);
            File.AppendAllText(ResultsFilePath, json + Environment.NewLine);
        }
    }
}
