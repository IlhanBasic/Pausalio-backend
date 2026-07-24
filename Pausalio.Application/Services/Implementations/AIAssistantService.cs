using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pausalio.Application.DTOs.AIAssistant;
using Pausalio.Application.Helpers;
using Pausalio.Application.Helpers.Pausalio.Application.Helpers;
using Pausalio.Application.Services.Implementations.AIAssistant;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly IEncryptionService _encryption;

        public AIAssistantService(
            IFinancialContextService financialContextService,
            IInvoiceService invoiceService,
            IExpenseService expenseService,
            IPaymentService paymentService,
            IEncryptionService encryptionService,
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
            _encryption = encryptionService;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
            _dataLoader = new AIAssistantDataLoader(invoiceService, expenseService, taxObligationService, paymentService);
            _toolExecutor = new AIAssistantToolExecutor(loggerFactory.CreateLogger<AIAssistantToolExecutor>());
            _responseParser = new OpenRouterResponseParser();
        }

        public async Task<AIResponseDto> SendMessageAsync(UserChatMessage message)
        {
            var userIdString = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                throw new UnauthorizedAccessException("Unable to determine current user.");

            var userProfile = await _unitOfWork.UserProfileRepository.GetByIdAsync(userId);
            if (userProfile == null)
                throw new UnauthorizedAccessException("User profile not found.");

            if (string.IsNullOrWhiteSpace(userProfile.OpenRouterApiKey) || string.IsNullOrWhiteSpace(userProfile.OpenRouterModelName))
                throw new InvalidOperationException("OpenRouter API key or model name not configured for your account.");

            var userBusinessProfiles = await _unitOfWork.UserBusinessProfileRepository
                .FindAllAsync(ubp => ubp.UserId == userId);

            var userBusinessProfile = userBusinessProfiles.FirstOrDefault();

            if (userBusinessProfile == null)
                throw new InvalidOperationException("Korisnik nema povezan biznis profil.");

            AiConversation? conversation = null;

            if (message.ConversationId.HasValue && message.ConversationId.Value != Guid.Empty)
            {
                conversation = await _unitOfWork.AiConversationRepository.GetByIdAsync(message.ConversationId.Value);
            }

            if (conversation == null || conversation.UserId != userId || conversation.IsDeleted)
            {
                var generatedTitle = message.Message.Length > 40
                    ? message.Message.Substring(0, 37) + "..."
                    : message.Message;

                conversation = new AiConversation
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    BusinessProfileId = userBusinessProfile.BusinessProfileId,
                    Title = string.IsNullOrWhiteSpace(generatedTitle) ? "Novi razgovor" : generatedTitle,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _unitOfWork.AiConversationRepository.AddAsync(conversation);
            }
            else
            {
                conversation.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.AiConversationRepository.Update(conversation);
            }

            var userAiMessage = new AiMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                Role = "user",
                Content = _encryption.Encrypt(message.Message),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AiMessageRepository.AddAsync(userAiMessage);

            await _unitOfWork.SaveChangesAsync();

            var financialContext = await _financialContextService.BuildContextAsync();
            var systemPrompt = AIAssistantPromptHelper.BuildSystemPrompt(financialContext);

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            if (message.History != null)
            {
                foreach (var item in message.History)
                {
                    messages.Add(new { role = item.Role, content = item.Content });
                }
            }

            messages.Add(new { role = "user", content = message.Message });

            var tools = AIToolsDefinition.GetTools();
            var cachedData = await _dataLoader.LoadAllDataAsync();
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

                request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    _encryption.Decrypt(userProfile.OpenRouterApiKey));

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var parsedResponse = _responseParser.Parse(responseString);

                if (parsedResponse.FinishReason != "tool_calls")
                {
                    var finalAnswer = parsedResponse.AssistantMessage ?? "Nije moguće dobiti odgovor.";

                    await SaveAssistantMessageAsync(conversation.Id, finalAnswer);

                    return new AIResponseDto
                    {
                        ConversationId = conversation.Id,
                        Message = finalAnswer,
                        Usage = parsedResponse.Usage
                    };
                }

                if (parsedResponse.AssistantMessageObject != null)
                {
                    messages.Add(parsedResponse.AssistantMessageObject);
                }

                if (!parsedResponse.ToolCallRawMessages.Any())
                {
                    var finalAnswer = parsedResponse.AssistantMessage ?? "Nije moguće dobiti odgovor.";
                    await SaveAssistantMessageAsync(conversation.Id, finalAnswer);

                    return new AIResponseDto
                    {
                        ConversationId = conversation.Id,
                        Message = finalAnswer,
                        Usage = parsedResponse.Usage
                    };
                }

                toolCallRound++;
                if (toolCallRound > MaxToolCallRounds)
                {
                    _logger.LogWarning("AI assistant reached maximum tool call rounds ({MaxToolCallRounds})", MaxToolCallRounds);
                    var fallbackAnswer = "Maksimalan broj poziva alata je dostignut. Molimo pokušajte ponovo s preciznijim upitom.";

                    await SaveAssistantMessageAsync(conversation.Id, fallbackAnswer);

                    return new AIResponseDto
                    {
                        ConversationId = conversation.Id,
                        Message = fallbackAnswer,
                        Usage = parsedResponse.Usage
                    };
                }

                foreach (var rawToolCall in parsedResponse.ToolCallRawMessages)
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
                        MessageId = userAiMessage.Id,
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

                    messages.Add(new
                    {
                        role = "tool",
                        tool_call_id = toolCallId,
                        content = toolResult
                    });
                }

                await _unitOfWork.SaveChangesAsync();

                continue;
            }
        }

        public async Task StreamMessageAsync(UserChatMessage message, Func<AiStreamChunkDto, Task> onChunk, CancellationToken cancellationToken = default)
        {
            var userIdString = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                throw new UnauthorizedAccessException("Unable to determine current user.");

            var userProfile = await _unitOfWork.UserProfileRepository.GetByIdAsync(userId);
            if (userProfile == null)
                throw new UnauthorizedAccessException("User profile not found.");

            if (string.IsNullOrWhiteSpace(userProfile.OpenRouterApiKey) || string.IsNullOrWhiteSpace(userProfile.OpenRouterModelName))
                throw new InvalidOperationException("OpenRouter API key or model name not configured for your account.");

            var userBusinessProfiles = await _unitOfWork.UserBusinessProfileRepository
                .FindAllAsync(ubp => ubp.UserId == userId);

            var userBusinessProfile = userBusinessProfiles.FirstOrDefault();

            if (userBusinessProfile == null)
                throw new InvalidOperationException("Korisnik nema povezan biznis profil.");

            AiConversation? conversation = null;

            if (message.ConversationId.HasValue && message.ConversationId.Value != Guid.Empty)
            {
                conversation = await _unitOfWork.AiConversationRepository.GetByIdAsync(message.ConversationId.Value);
            }

            if (conversation == null || conversation.UserId != userId || conversation.IsDeleted)
            {
                var generatedTitle = message.Message.Length > 40
                    ? message.Message.Substring(0, 37) + "..."
                    : message.Message;

                conversation = new AiConversation
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    BusinessProfileId = userBusinessProfile.BusinessProfileId,
                    Title = string.IsNullOrWhiteSpace(generatedTitle) ? "Novi razgovor" : generatedTitle,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _unitOfWork.AiConversationRepository.AddAsync(conversation);
            }
            else
            {
                conversation.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.AiConversationRepository.Update(conversation);
            }

            var userAiMessage = new AiMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                Role = "user",
                Content = _encryption.Encrypt(message.Message),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AiMessageRepository.AddAsync(userAiMessage);
            await _unitOfWork.SaveChangesAsync();

            var financialContext = await _financialContextService.BuildContextAsync();
            var systemPrompt = AIAssistantPromptHelper.BuildSystemPrompt(financialContext);

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            if (message.History != null)
            {
                foreach (var item in message.History)
                {
                    messages.Add(new { role = item.Role, content = item.Content });
                }
            }

            messages.Add(new { role = "user", content = message.Message });
            var tools = AIToolsDefinition.GetTools();
            var cachedData = await _dataLoader.LoadAllDataAsync();
            var toolCallRound = 0;
            var assistantBuffer = new StringBuilder();

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(cancellationToken);
                }

                var requestBody = new
                {
                    model = userProfile.OpenRouterModelName,
                    messages,
                    tools,
                    tool_choice = "auto",
                    max_tokens = 1000,
                    temperature = 0.7,
                    stream = true
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, _configuration.Value.ApiUrl)
                {
                    Content = content
                };

                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    _encryption.Decrypt(userProfile.OpenRouterApiKey));

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var parsedResponse = await ParseOpenRouterStreamAsync(responseStream, assistantBuffer, conversation.Id, onChunk, cancellationToken);

                if (parsedResponse.FinishReason != "tool_calls")
                {
                    var finalAnswer = parsedResponse.AssistantMessage ?? assistantBuffer.ToString();
                    if (string.IsNullOrWhiteSpace(finalAnswer))
                        finalAnswer = "Nije moguće dobiti odgovor.";

                    await SaveAssistantMessageAsync(conversation.Id, finalAnswer);
                    await onChunk(new AiStreamChunkDto
                    {
                        ConversationId = conversation.Id,
                        Type = "final",
                        Content = finalAnswer,
                        IsFinal = true,
                        Usage = parsedResponse.Usage
                    });

                    return;
                }

                if (parsedResponse.AssistantMessageObject != null)
                {
                    messages.Add(parsedResponse.AssistantMessageObject);
                }

                if (!parsedResponse.ToolCallRawMessages.Any())
                {
                    var finalAnswer = parsedResponse.AssistantMessage ?? assistantBuffer.ToString();
                    if (string.IsNullOrWhiteSpace(finalAnswer))
                        finalAnswer = "Nije moguće dobiti odgovor.";

                    await SaveAssistantMessageAsync(conversation.Id, finalAnswer);
                    await onChunk(new AiStreamChunkDto
                    {
                        ConversationId = conversation.Id,
                        Type = "final",
                        Content = finalAnswer,
                        IsFinal = true,
                        Usage = parsedResponse.Usage
                    });

                    return;
                }

                toolCallRound++;
                if (toolCallRound > MaxToolCallRounds)
                {
                    _logger.LogWarning("AI assistant reached maximum tool call rounds ({MaxToolCallRounds})", MaxToolCallRounds);
                    var fallbackAnswer = "Maksimalan broj poziva alata je dostignut. Molimo pokušajte ponovo s preciznijim upitom.";

                    await SaveAssistantMessageAsync(conversation.Id, fallbackAnswer);
                    await onChunk(new AiStreamChunkDto
                    {
                        ConversationId = conversation.Id,
                        Type = "final",
                        Content = fallbackAnswer,
                        IsFinal = true,
                        Usage = parsedResponse.Usage
                    });

                    return;
                }

                foreach (var rawToolCall in parsedResponse.ToolCallRawMessages)
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
                        MessageId = userAiMessage.Id,
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
                    await onChunk(new AiStreamChunkDto
                    {
                        ConversationId = conversation.Id,
                        Type = "tool_result",
                        Content = toolResult,
                        IsFinal = false
                    });

                    messages.Add(new
                    {
                        role = "tool",
                        tool_call_id = toolCallId,
                        content = toolResult
                    });
                }

                await _unitOfWork.SaveChangesAsync();
            }
        }

        private async Task<OpenRouterResponse> ParseOpenRouterStreamAsync(
            Stream stream,
            StringBuilder assistantBuffer,
            Guid conversationId,
            Func<AiStreamChunkDto, Task> onChunk,
            CancellationToken cancellationToken)
        {
            var finishReason = string.Empty;
            AIUsageDto? usage = null;
            using var reader = new StreamReader(stream, Encoding.UTF8);

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

                        if (choice.TryGetProperty("delta", out var delta)
                            && delta.TryGetProperty("content", out var contentElement)
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

            return new OpenRouterResponse
            {
                FinishReason = string.IsNullOrEmpty(finishReason) ? "unknown" : finishReason,
                AssistantMessage = assistantBuffer.ToString(),
                MessageRaw = assistantBuffer.ToString(),
                AssistantMessageObject = null,
                ToolCallRawMessages = Array.Empty<string>(),
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

        private async Task SaveAssistantMessageAsync(Guid conversationId, string content)
        {
            var assistantAiMessage = new AiMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Role = "assistant",
                Content = _encryption.Encrypt(content),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.AiMessageRepository.AddAsync(assistantAiMessage);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<AiConversationDto>> GetConversationsAsync()
        {
            var userIdString = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                throw new UnauthorizedAccessException("Unable to determine current user.");

            var conversations = await _unitOfWork.AiConversationRepository.FindAllAsync(
                c => c.UserId == userId && !c.IsDeleted
            );

            return conversations
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                .Select(c => new AiConversationDto
                {
                    Id = c.Id,
                    Title = c.Title ?? "Novi razgovor",
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToList();
        }

        public async Task<List<AiMessageDto>> GetConversationMessagesAsync(Guid conversationId)
        {
            var userIdString = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                throw new UnauthorizedAccessException("Unable to determine current user.");

            var conversation = await _unitOfWork.AiConversationRepository.GetByIdAsync(conversationId);
            if (conversation == null || conversation.UserId != userId || conversation.IsDeleted)
                throw new KeyNotFoundException("Razgovor nije pronađen.");

            var messages = await _unitOfWork.AiMessageRepository.FindAllAsync(m => m.ConversationId == conversationId);

            return messages
                .Where(m => m.Role == "user" || m.Role == "assistant")
                .Where(m => !string.IsNullOrEmpty(m.Content))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new AiMessageDto
                {
                    Id = m.Id,
                    Role = m.Role,
                    Content = _encryption.Decrypt(m.Content),
                    CreatedAt = m.CreatedAt
                })
                .ToList();
        }

        public async Task DeleteConversationAsync(Guid conversationId)
        {
            var userIdString = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                throw new UnauthorizedAccessException("Unable to determine current user.");

            var conversation = await _unitOfWork.AiConversationRepository.GetByIdAsync(conversationId);
            if (conversation == null || conversation.UserId != userId)
                throw new KeyNotFoundException("Razgovor nije pronađen.");

            conversation.IsDeleted = true;
            conversation.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}