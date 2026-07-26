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
        private readonly OpenRouterClientService _openRouterClient;
        private readonly OpenRouterStreamParser _streamParser;
        private readonly ToolCallProcessor _toolCallProcessor;

        public AIAssistantService(
            IFinancialContextService financialContextService,
            IInvoiceService invoiceService,
            IExpenseService expenseService,
            IPaymentService paymentService,
            IEncryptionService encryptionService,
            ITaxObligationService taxObligationService,
            IReminderService reminderService,
            IClientService clientService,
            IBankAccountService bankAccountService,
            IBusinessProfileService businessProfileService,
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
            _dataLoader = new AIAssistantDataLoader(
                invoiceService, 
                expenseService, 
                taxObligationService, 
                paymentService,
                reminderService,
                clientService,
                bankAccountService,
                businessProfileService,
                currentUserService);
            _toolExecutor = new AIAssistantToolExecutor(loggerFactory.CreateLogger<AIAssistantToolExecutor>());
            _responseParser = new OpenRouterResponseParser();
            _openRouterClient = new OpenRouterClientService(httpClient, configuration, encryptionService);
            _streamParser = new OpenRouterStreamParser();
            _toolCallProcessor = new ToolCallProcessor(unitOfWork, _toolExecutor, logger);
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

                var rawTitle = string.IsNullOrWhiteSpace(generatedTitle) ? "Novi razgovor" : generatedTitle;

                conversation = new AiConversation
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    BusinessProfileId = userBusinessProfile.BusinessProfileId,
                    Title = _encryption.Encrypt(rawTitle),
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
            CachedToolData? cachedData = null;
            var toolCallRound = 0;

            // Accumulate token usage across all tool-call rounds
            int accPromptTokens = 0, accCompletionTokens = 0, accTotalTokens = 0;

            while (true)
            {
                var response = await _openRouterClient.SendRequestAsync(
                    userProfile.OpenRouterApiKey,
                    userProfile.OpenRouterModelName,
                    messages,
                    tools,
                    stream: false,
                    cancellationToken: CancellationToken.None,
                    temperature: message.Temperature ?? 0.2);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var parsedResponse = _responseParser.Parse(responseString);

                // Accumulate tokens from this round
                if (parsedResponse.Usage != null)
                {
                    accPromptTokens += parsedResponse.Usage.PromptTokens;
                    accCompletionTokens += parsedResponse.Usage.CompletionTokens;
                    accTotalTokens += parsedResponse.Usage.TotalTokens;
                }

                if (parsedResponse.FinishReason != "tool_calls")
                {
                    var finalAnswer = parsedResponse.AssistantMessage ?? "Nije moguće dobiti odgovor.";

                    await SaveAssistantMessageAsync(conversation.Id, finalAnswer);

                    return new AIResponseDto
                    {
                        ConversationId = conversation.Id,
                        Message = finalAnswer,
                        Usage = new AIUsageDto { PromptTokens = accPromptTokens, CompletionTokens = accCompletionTokens, TotalTokens = accTotalTokens }
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
                        Usage = new AIUsageDto { PromptTokens = accPromptTokens, CompletionTokens = accCompletionTokens, TotalTokens = accTotalTokens }
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
                        Usage = new AIUsageDto { PromptTokens = accPromptTokens, CompletionTokens = accCompletionTokens, TotalTokens = accTotalTokens }
                    };
                }

                foreach (var rawToolCall in parsedResponse.ToolCallRawMessages)
                {
                    if (cachedData == null)
                    {
                        cachedData = await _dataLoader.LoadAllDataAsync();
                    }

                    var toolResult = await _toolCallProcessor.ProcessToolCallAsync(
                        rawToolCall,
                        userAiMessage.Id,
                        toolCallRound,
                        cachedData);

                    messages.Add(toolResult.ToolMessage);
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

                var rawTitle = string.IsNullOrWhiteSpace(generatedTitle) ? "Novi razgovor" : generatedTitle;

                conversation = new AiConversation
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    BusinessProfileId = userBusinessProfile.BusinessProfileId,
                    Title = _encryption.Encrypt(rawTitle),
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
            CachedToolData? cachedData = null;
            var toolCallRound = 0;

            // Accumulate token usage across all tool-call rounds
            int accPromptTokens = 0, accCompletionTokens = 0, accTotalTokens = 0;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(cancellationToken);
                }

                var assistantBuffer = new StringBuilder();

                using var response = await _openRouterClient.SendRequestAsync(
                    userProfile.OpenRouterApiKey,
                    userProfile.OpenRouterModelName,
                    messages,
                    tools,
                    stream: true,
                    cancellationToken: CancellationToken.None,
                    temperature: message.Temperature ?? 0.2);
                response.EnsureSuccessStatusCode();

                using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var parsedResponse = await _streamParser.ParseStreamAsync(responseStream, assistantBuffer, conversation.Id, onChunk, cancellationToken);

                // Accumulate tokens from this round
                if (parsedResponse.Usage != null)
                {
                    accPromptTokens += parsedResponse.Usage.PromptTokens;
                    accCompletionTokens += parsedResponse.Usage.CompletionTokens;
                    accTotalTokens += parsedResponse.Usage.TotalTokens;
                }

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
                        Usage = new AIUsageDto { PromptTokens = accPromptTokens, CompletionTokens = accCompletionTokens, TotalTokens = accTotalTokens }
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
                        Usage = new AIUsageDto { PromptTokens = accPromptTokens, CompletionTokens = accCompletionTokens, TotalTokens = accTotalTokens }
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
                        Usage = new AIUsageDto { PromptTokens = accPromptTokens, CompletionTokens = accCompletionTokens, TotalTokens = accTotalTokens }
                    });

                    return;
                }

                foreach (var rawToolCall in parsedResponse.ToolCallRawMessages)
                {
                    if (cachedData == null)
                    {
                        cachedData = await _dataLoader.LoadAllDataAsync();
                    }

                    var toolResult = await _toolCallProcessor.ProcessToolCallAsync(
                        rawToolCall,
                        userAiMessage.Id,
                        toolCallRound,
                        cachedData);

                    await onChunk(new AiStreamChunkDto
                    {
                        ConversationId = conversation.Id,
                        Type = "tool_result",
                        Content = toolResult.ToolResult,
                        IsFinal = false
                    });

                    messages.Add(toolResult.ToolMessage);
                }

                await _unitOfWork.SaveChangesAsync();
            }
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
                .Select(c =>
                {
                    string decryptedTitle;
                    try
                    {
                        decryptedTitle = !string.IsNullOrEmpty(c.Title) ? _encryption.Decrypt(c.Title) : "Novi razgovor";
                    }
                    catch
                    {
                        // Fallback ukoliko postoji stari neenkriptovan naslov u bazi
                        decryptedTitle = c.Title ?? "Novi razgovor";
                    }

                    return new AiConversationDto
                    {
                        Id = c.Id,
                        Title = decryptedTitle,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    };
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
                .Select(m =>
                {
                    string decryptedContent;
                    try
                    {
                        decryptedContent = _encryption.Decrypt(m.Content ?? "");
                    }
                    catch
                    {
                        decryptedContent = m.Content ?? "";
                    }

                    return new AiMessageDto
                    {
                        Id = m.Id,
                        Role = m.Role,
                        Content = decryptedContent,
                        CreatedAt = m.CreatedAt
                    };
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