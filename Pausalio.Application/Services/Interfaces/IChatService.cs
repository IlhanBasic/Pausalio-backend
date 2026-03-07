using Pausalio.Application.DTOs.ChatMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IChatService
    {
        Task<ChatMessageDto> SendMessageAsync(Guid senderId, Guid receiverId, Guid businessId, string content);
        Task<IEnumerable<ChatMessageDto>> GetHistoryAsync(Guid userId1, Guid userId2, Guid businessId, int page = 1, int pageSize = 30);
        Task<IEnumerable<BusinessMemberDto>> GetBusinessMembersAsync(Guid businessId, Guid excludeUserId);
        Task MarkAsDeliveredAsync(Guid receiverId, Guid senderId, Guid businessId);
        Task MarkAsReadAsync(Guid readerId, Guid senderId, Guid businessId);
        Task<int> GetUnreadCountAsync(Guid userId, Guid businessId);
    }
}
