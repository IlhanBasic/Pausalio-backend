using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pausalio.Application.DTOs.AIAssistant;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Evaluation.Models;
using Pausalio.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pausalio.Evaluation
{
    public class TemperatureEvaluator
    {
        private readonly IAIAssistantService _aiAssistantService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TemperatureEvaluator> _logger;

        private readonly double[] _temperatures =
        {
            0.0,
            0.7,
            1.0
        };

        public TemperatureEvaluator(
            IAIAssistantService aiAssistantService,
            IUnitOfWork unitOfWork,
            ILogger<TemperatureEvaluator> logger)
        {
            _aiAssistantService = aiAssistantService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task RunAsync(List<EvalQuestion> questions)
        {
            foreach (var temperature in _temperatures)
            {
                var file = $"temperature_{temperature.ToString().Replace(".", "_")}.jsonl";

                var processedIds = LoadExistingQuestionIds(file);

                _logger.LogInformation(
                    "Starting temperature evaluation. Temperature: {Temperature}. Already processed: {Count}/{Total}",
                    temperature,
                    processedIds.Count,
                    questions.Count);

                foreach (var question in questions)
                {
                    if (processedIds.Contains(question.Id))
                    {
                        _logger.LogInformation(
                            "Skipping question {Id} at temperature {Temperature} (already processed)",
                            question.Id,
                            temperature);

                        continue;
                    }

                    try
                    {
                        _logger.LogInformation(
                            "Processing question {Id} at temperature {Temperature}",
                            question.Id,
                            temperature);

                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        var response = await _aiAssistantService.SendMessageAsync(
                            new UserChatMessage
                            {
                                ConversationId = Guid.Empty,
                                Message = question.Question,
                                History = new List<ChatHistoryItem>(),
                                Temperature = temperature
                            });

                        stopwatch.Stop();

                        var context = _unitOfWork.GetContext();

                        var dbToolCalls = await context.Set<AiToolCall>()
                            .Include(tc => tc.Message)
                            .Where(tc => tc.Message.ConversationId == response.ConversationId)
                            .OrderBy(tc => tc.CreatedAt)
                            .ToListAsync();

                        var actualToolCalls = dbToolCalls.Select(tc => new ToolCallInfo
                        {
                            ToolName = tc.ToolName,
                            Arguments = tc.Arguments,
                            Result = tc.Result ?? string.Empty,
                            Success = tc.Success,
                            RoundNumber = tc.RoundNumber,
                            DurationMs = tc.DurationMs ?? 0
                        }).ToList();

                        var result = new EvalResult
                        {
                            QuestionId = question.Id,
                            Category = question.Category,
                            Question = question.Question,

                            AssistantResponse = response.Message,

                            ExpectedTools = question.ExpectedTools,
                            ExpectedParameters = question.ExpectedParameters,

                            ActualToolCalls = actualToolCalls,

                            ConversationId = response.ConversationId.ToString(),

                            ExecutionDurationMs = stopwatch.ElapsedMilliseconds,

                            PromptTokens = response.Usage?.PromptTokens ?? 0,
                            CompletionTokens = response.Usage?.CompletionTokens ?? 0,

                            Temperature = temperature
                        };

                        MetricsCalculator.Calculate(result);

                        AppendResult(file, result);

                        processedIds.Add(question.Id);

                        _logger.LogInformation(
                            "Finished question {Id} at temperature {Temperature}",
                            question.Id,
                            temperature);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Temperature {Temperature}, Question {Id} failed",
                            temperature,
                            question.Id);
                    }
                }

                _logger.LogInformation(
                    "Temperature {Temperature} completed. Results saved to {File}",
                    temperature,
                    file);
            }

            _logger.LogInformation(
                "All temperature evaluations completed.");
        }


        private HashSet<int> LoadExistingQuestionIds(string file)
        {
            var ids = new HashSet<int>();

            if (!File.Exists(file))
            {
                return ids;
            }

            foreach (var line in File.ReadLines(file))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                try
                {
                    using var doc = JsonDocument.Parse(line);

                    if (doc.RootElement.TryGetProperty(
                        "QuestionId",
                        out var idProperty))
                    {
                        ids.Add(idProperty.GetInt32());
                    }
                }
                catch
                {
                    // Ignoriši neispravne JSONL linije
                }
            }

            return ids;
        }


        private void AppendResult(string file, EvalResult result)
        {
            var json = JsonSerializer.Serialize(result);

            File.AppendAllText(
                file,
                json + Environment.NewLine);
        }
    }
}