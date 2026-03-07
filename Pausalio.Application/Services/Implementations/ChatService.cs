using Pausalio.Application.DTOs.ChatMessage;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Implementations
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ChatMessageDto> SendMessageAsync(
            Guid senderId, Guid receiverId, Guid businessId, string content)
        {
            var senderInBusiness = await _unitOfWork.UserBusinessProfileRepository
                .FindFirstOrDefaultAsync(x => x.UserId == senderId && x.BusinessProfileId == businessId);

            var receiverInBusiness = await _unitOfWork.UserBusinessProfileRepository
                .FindFirstOrDefaultAsync(x => x.UserId == receiverId && x.BusinessProfileId == businessId);

            if (senderInBusiness == null || receiverInBusiness == null)
                throw new UnauthorizedAccessException("Korisnici nisu u istom biznisu.");

            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                BusinessProfileId = businessId,
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content.Trim(),
                Status = MessageStatus.Sent,
                SentAt = DateTime.UtcNow
            };

            await _unitOfWork.ChatMessageRepository.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            // Učitaj sender info za DTO
            var sender = await _unitOfWork.UserProfileRepository
                .FindFirstOrDefaultAsync(x => x.Id == senderId);
            var receiver = await _unitOfWork.UserProfileRepository
                .FindFirstOrDefaultAsync(x => x.Id == receiverId);

            return MapToDto(message, sender!, receiver!);
        }

        public async Task<IEnumerable<ChatMessageDto>> GetHistoryAsync(
            Guid userId1, Guid userId2, Guid businessId, int page = 1, int pageSize = 30)
        {
            var messages = await _unitOfWork.ChatMessageRepository
                .FindAllAsync(x =>
                    x.BusinessProfileId == businessId &&
                    !x.IsDeleted &&
                    ((x.SenderId == userId1 && x.ReceiverId == userId2) ||
                     (x.SenderId == userId2 && x.ReceiverId == userId1)));

            var user1 = await _unitOfWork.UserProfileRepository
                .FindFirstOrDefaultAsync(x => x.Id == userId1);
            var user2 = await _unitOfWork.UserProfileRepository
                .FindFirstOrDefaultAsync(x => x.Id == userId2);

            return messages
                .OrderByDescending(x => x.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(x => x.SentAt)
                .Select(x =>
                {
                    var sender = x.SenderId == userId1 ? user1! : user2!;
                    var receiver = x.ReceiverId == userId1 ? user1! : user2!;
                    return MapToDto(x, sender, receiver);
                });
        }

        public async Task<IEnumerable<BusinessMemberDto>> GetBusinessMembersAsync(
            Guid businessId, Guid excludeUserId)
        {
            var members = await _unitOfWork.UserBusinessProfileRepository
                .FindAllAsync(x => x.BusinessProfileId == businessId && x.UserId != excludeUserId);

            var result = new List<BusinessMemberDto>();
            foreach (var member in members)
            {
                var user = await _unitOfWork.UserProfileRepository
                    .FindFirstOrDefaultAsync(x => x.Id == member.UserId);

                if (user != null)
                {
                    result.Add(new BusinessMemberDto
                    {
                        UserId = user.Id,
                        FullName = $"{user.FirstName} {user.LastName}",
                        ProfilePicture = user.ProfilePicture,
                        Role = member.Role.ToString()
                    });
                }
            }

            return result;
        }

        public async Task MarkAsDeliveredAsync(Guid receiverId, Guid senderId, Guid businessId)
        {
            var messages = await _unitOfWork.ChatMessageRepository
                .FindAllAsync(x =>
                    x.SenderId == senderId &&
                    x.ReceiverId == receiverId &&
                    x.BusinessProfileId == businessId &&
                    x.Status == MessageStatus.Sent);

            foreach (var msg in messages)
            {
                msg.Status = MessageStatus.Delivered;
                msg.DeliveredAt = DateTime.UtcNow;
                _unitOfWork.ChatMessageRepository.Update(msg);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(Guid readerId, Guid senderId, Guid businessId)
        {
            var messages = await _unitOfWork.ChatMessageRepository
                .FindAllAsync(x =>
                    x.SenderId == senderId &&
                    x.ReceiverId == readerId &&
                    x.BusinessProfileId == businessId &&
                    x.Status != MessageStatus.Read);

            foreach (var msg in messages)
            {
                msg.Status = MessageStatus.Read;
                msg.ReadAt = DateTime.UtcNow;
                _unitOfWork.ChatMessageRepository.Update(msg);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId, Guid businessId)
        {
            var messages = await _unitOfWork.ChatMessageRepository
                .FindAllAsync(x =>
                    x.ReceiverId == userId &&
                    x.BusinessProfileId == businessId &&
                    x.Status != MessageStatus.Read &&
                    !x.IsDeleted);

            return messages.Count;
        }

        private static ChatMessageDto MapToDto(ChatMessage message, UserProfile sender, UserProfile receiver)
        {
            return new ChatMessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderName = $"{sender.FirstName} {sender.LastName}",
                SenderAvatar = sender.ProfilePicture,
                ReceiverId = message.ReceiverId,
                ReceiverName = $"{receiver.FirstName} {receiver.LastName}",
                Content = message.Content,
                Status = (int)message.Status,
                SentAt = message.SentAt,
                ReadAt = message.ReadAt
            };
        }
    }
}
