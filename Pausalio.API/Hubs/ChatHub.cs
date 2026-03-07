using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pausalio.Application.Services.Interfaces;
using System.Security.Claims;

namespace Pausalio.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
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

            await Clients.Group(roomKey).SendAsync("ReceiveMessage", message);
            await Clients.Group($"user-{receiverId}-{businessId}").SendAsync("NewMessageNotification", message);

            Console.WriteLine($"SendMessage: poruka poslata u sobu {roomKey}");
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