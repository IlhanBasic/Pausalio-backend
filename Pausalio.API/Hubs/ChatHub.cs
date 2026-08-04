using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pausalio.Application.DTOs.ChatMessage;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Infrastructure.Persistence;
using System.Net;
using System.Security.Claims;

namespace Pausalio.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IChatConnectionManager _connectionManager;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(
            IChatService chatService,
            IChatConnectionManager connectionManager,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService,
            IServiceScopeFactory scopeFactory,
            ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _connectionManager = connectionManager;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                _connectionManager.AddConnection(userId, Context.ConnectionId);
                _logger.LogInformation("SignalR connection tracked. UserId: {UserId}, ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                _connectionManager.RemoveConnection(userId, Context.ConnectionId);
                _logger.LogInformation("SignalR connection removed. UserId: {UserId}, ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChat(string otherUserId, string businessId)
        {
            var myUserId = GetCurrentUserId();
            Console.WriteLine($"JoinChat: myUserId={myUserId}, otherUserId={otherUserId}, businessId={businessId}");

            if (string.IsNullOrEmpty(myUserId) ||
                string.IsNullOrEmpty(otherUserId) ||
                string.IsNullOrEmpty(businessId))
            {
                Console.WriteLine("JoinChat: jedan od parametara je null!");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{myUserId}-{businessId}");

            var roomKey = GetRoomKey(myUserId, otherUserId, businessId);
            Console.WriteLine($"JoinChat: roomKey={roomKey}");
            await Groups.AddToGroupAsync(Context.ConnectionId, roomKey);

            await _chatService.MarkAsDeliveredAsync(
                Guid.Parse(myUserId),
                Guid.Parse(otherUserId),
                Guid.Parse(businessId));
        }

        public async Task SendMessage(string receiverId, string businessId, string content)
        {
            var senderId = GetCurrentUserId();
            Console.WriteLine($"SendMessage: senderId={senderId}, receiverId={receiverId}, businessId={businessId}");

            if (string.IsNullOrEmpty(senderId) ||
                string.IsNullOrEmpty(receiverId) ||
                string.IsNullOrEmpty(businessId) ||
                string.IsNullOrEmpty(content))
                return;

            var message = await _chatService.SendMessageAsync(
                Guid.Parse(senderId),
                Guid.Parse(receiverId),
                Guid.Parse(businessId),
                content);

            var roomKey = GetRoomKey(senderId, receiverId, businessId);
            Console.WriteLine($"SendMessage: roomKey={roomKey}");

            await Clients.Caller.SendAsync("ReceiveMessage", message);
            await Clients.Group(roomKey).SendAsync("ReceiveMessage", message);

            if (_connectionManager.IsOnline(receiverId))
            {
                await Clients.Group($"user-{receiverId}-{businessId}").SendAsync("NewMessageNotification", message);
                Console.WriteLine($"SendMessage: poruka poslata u sobu {roomKey}");
                return;
            }

            _logger.LogInformation(
                "Recipient {ReceiverId} is offline. Scheduling email fallback for unread message from {SenderName}.",
                receiverId,
                message.SenderName);

            if (!_connectionManager.ShouldSendOfflineEmailNotification(receiverId))
            {
                _logger.LogInformation("Offline email fallback skipped for recipient {ReceiverId} because the cooldown window is still active.", receiverId);
                return;
            }

            _ = SendUnreadMessageEmailFallbackAsync(message, receiverId);
        }

        public async Task LeaveChat(string otherUserId, string businessId)
        {
            var myUserId = GetCurrentUserId();

            if (string.IsNullOrEmpty(myUserId) ||
                string.IsNullOrEmpty(otherUserId) ||
                string.IsNullOrEmpty(businessId))
                return;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{myUserId}-{businessId}");

            var roomKey = GetRoomKey(myUserId, otherUserId, businessId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomKey);
        }

        public async Task MarkAsRead(string senderId, string businessId)
        {
            var readerId = GetCurrentUserId();

            if (string.IsNullOrEmpty(readerId) ||
                string.IsNullOrEmpty(senderId) ||
                string.IsNullOrEmpty(businessId))
                return;

            await _chatService.MarkAsReadAsync(
                Guid.Parse(readerId),
                Guid.Parse(senderId),
                Guid.Parse(businessId));

            var roomKey = GetRoomKey(readerId, senderId, businessId);

            await Clients.Group(roomKey).SendAsync("MessagesRead", new
            {
                readBy = readerId,
                businessId
            });
        }

        private async Task SendUnreadMessageEmailFallbackAsync(ChatMessageDto message, string receiverId)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<PausalioDbContext>();
                var receiver = await dbContext.UserProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == Guid.Parse(receiverId));

                if (receiver == null || string.IsNullOrWhiteSpace(receiver.Email))
                {
                    _logger.LogWarning("Unable to send unread message email fallback because recipient {ReceiverId} has no email address.", receiverId);
                    return;
                }

                var senderName = string.IsNullOrWhiteSpace(message.SenderName) ? "Korisnik" : message.SenderName;
                var appUrl = "https://app-pausalio.netlify.app";
                var emailBody = _emailTemplateService.GetUnreadMessageNotificationTemplate(
                    WebUtility.HtmlEncode(senderName),
                    appUrl);

                await _emailService.SendEmailAsync(receiver.Email, "Nova poruka na platformi Paušalio", emailBody);
                _logger.LogInformation("Unread message email fallback sent to {RecipientEmail}.", receiver.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send unread message email fallback for recipient {ReceiverId}.", receiverId);
            }
        }

        private string? GetCurrentUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private static string GetRoomKey(string userId1, string userId2, string businessId)
        {
            var sorted = new[] { userId1, userId2 }.OrderBy(x => x).ToArray();
            return $"chat-{sorted[0]}-{sorted[1]}-{businessId}";
        }
    }
}