using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pausalio.Application.DTOs.AIAssistant;
using Pausalio.Application.Helpers;
using Pausalio.Application.Helpers.Pausalio.Application.Helpers;
using Pausalio.Application.Services.Implementations.AIAssistant;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Configuration;

namespace Pausalio.Application.Services.Implementations
{
    public class AIAssistantService : IAIAssistantService
    {
        private const int MaxToolCallRounds = 6;

        private readonly IFinancialContextService _financialContextService;
        private readonly AIAssistantDataLoader _dataLoader;
        private readonly AIAssistantToolExecutor _toolExecutor;
        private readonly OpenRouterResponseParser _responseParser;
        private readonly IOptions<OpenRouterSettings> _configuration;
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AIAssistantService> _logger;

        public AIAssistantService(
            IFinancialContextService financialContextService,
            IInvoiceService invoiceService,
            IExpenseService expenseService,
            IPaymentService paymentService,
            ITaxObligationService taxObligationService,
            IOptions<OpenRouterSettings> configuration,
            HttpClient httpClient,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<AIAssistantService> logger,
            ILoggerFactory loggerFactory)
        {
            _financialContextService = financialContextService;
            _configuration = configuration;
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
            _dataLoader = new AIAssistantDataLoader(invoiceService, expenseService, taxObligationService, paymentService);
            _toolExecutor = new AIAssistantToolExecutor(loggerFactory.CreateLogger<AIAssistantToolExecutor>());
            _responseParser = new OpenRouterResponseParser();
        }

        public async Task<AIResponseDto> SendMessageAsync(UserChatMessage message)
        {
            var financialContext = await _financialContextService.BuildContextAsync();
            var systemPrompt = AIAssistantPromptHelper.BuildSystemPrompt(financialContext);

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            foreach (var item in message.History)
                messages.Add(new { role = item.Role, content = item.Content });

            messages.Add(new { role = "user", content = message.Message });
            var tools = AIToolsDefinition.GetTools();
            var cachedData = await _dataLoader.LoadAllDataAsync();

            var userIdString = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                throw new UnauthorizedAccessException("Unable to determine current user.");

            var userProfile = await _unitOfWork.UserProfileRepository.GetByIdAsync(userId);
            if (userProfile == null)
                throw new UnauthorizedAccessException("User profile not found.");

            if (string.IsNullOrWhiteSpace(userProfile.OpenRouterApiKey) || string.IsNullOrWhiteSpace(userProfile.OpenRouterModelName))
                throw new InvalidOperationException("OpenRouter API key or model name not configured for your account.");

            var toolCallRound = 0;

            while (true)
            {
                var requestBody = new
                {
                    model = userProfile.OpenRouterModelName,
                    messages,
                    tools,
                    tool_choice = "auto",
                    max_tokens = 1000,
                    temperature = 0.7
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, _configuration.Value.ApiUrl)
                {
                    Content = content
                };

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userProfile.OpenRouterApiKey);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var parsedResponse = _responseParser.Parse(responseString);

                if (parsedResponse.FinishReason != "tool_calls")
                {
                    return new AIResponseDto
                    {
                        Message = parsedResponse.AssistantMessage ?? "Nije moguće dobiti odgovor.",
                        Usage = parsedResponse.Usage
                    };
                }

                if (parsedResponse.AssistantMessageObject != null)
                {
                    messages.Add(parsedResponse.AssistantMessageObject);
                }

                if (!parsedResponse.ToolCallRawMessages.Any())
                {
                    return new AIResponseDto
                    {
                        Message = parsedResponse.AssistantMessage ?? "Nije moguće dobiti odgovor.",
                        Usage = parsedResponse.Usage
                    };
                }

                toolCallRound++;
                if (toolCallRound > MaxToolCallRounds)
                {
                    _logger.LogWarning("AI assistant reached maximum tool call rounds ({MaxToolCallRounds})", MaxToolCallRounds);
                    return new AIResponseDto
                    {
                        Message = "Maksimalan broj poziva alata je dostignut. Molimo pokušajte ponovo s preciznijim upitom.",
                        Usage = parsedResponse.Usage
                    };
                }

                foreach (var rawToolCall in parsedResponse.ToolCallRawMessages)
                {
                    string toolResult;
                    var toolCallId = "unknown";

                    try
                    {
                        using var toolDoc = JsonDocument.Parse(rawToolCall);
                        var toolCall = toolDoc.RootElement;
                        toolCallId = toolCall.TryGetProperty("id", out var idProp)
                            ? idProp.GetString() ?? "unknown"
                            : "unknown";

                        var functionElement = toolCall.GetProperty("function");
                        var functionName = functionElement.GetProperty("name").GetString()!;
                        var argumentsElement = functionElement.GetProperty("arguments");
                        var argumentsJson = argumentsElement.ValueKind == JsonValueKind.String
                            ? argumentsElement.GetString()!
                            : argumentsElement.GetRawText();

                        toolResult = _toolExecutor.ExecuteTool(functionName, argumentsJson, cachedData);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse or execute tool call {ToolCallId}", toolCallId);
                        toolResult = $"Invalid tool call or arguments: {ex.Message}";
                    }

                    messages.Add(new
                    {
                        role = "tool",
                        tool_call_id = toolCallId,
                        content = toolResult
                    });
                }

                continue;
            }
        }
    }
}
