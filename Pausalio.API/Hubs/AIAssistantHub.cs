using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pausalio.Application.DTOs.AIAssistant;
using Pausalio.Application.Services.Interfaces;
using System.Security.Claims;

namespace Pausalio.API.Hubs
{
    [Authorize]
    public class AIAssistantHub : Hub
    {
        private readonly IAIAssistantService _aiAssistantService;

        public AIAssistantHub(IAIAssistantService aiAssistantService)
        {
            _aiAssistantService = aiAssistantService;
        }

        public async Task SendAiMessage(UserChatMessage message)
        {
            var conversationId = message.ConversationId ?? Guid.Empty;

            try
            {
                await _aiAssistantService.StreamMessageAsync(
                    message,
                    async chunk => await Clients.Caller.SendAsync("ReceiveAiStreamChunk", chunk),
                    Context.ConnectionAborted);
            }
            catch (OperationCanceledException)
            {
                await Clients.Caller.SendAsync("ReceiveAiStreamChunk", new AiStreamChunkDto
                {
                    ConversationId = conversationId,
                    Type = "error",
                    Content = "AI stream was canceled.",
                    IsFinal = true
                });
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveAiStreamChunk", new AiStreamChunkDto
                {
                    ConversationId = conversationId,
                    Type = "error",
                    Content = ex.Message,
                    IsFinal = true
                });
                throw;
            }
        }

        private string? GetCurrentUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
