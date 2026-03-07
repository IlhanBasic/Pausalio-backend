using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.Services.Interfaces;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ICurrentUserService _currentUserService;

        public ChatController(IChatService chatService, ICurrentUserService currentUserService)
        {
            _chatService = chatService;
            _currentUserService = currentUserService;
        }

        [HttpGet("members")]
        public async Task<IActionResult> GetBusinessMembers()
        {
            var userId = Guid.Parse(_currentUserService.GetUserId()!);
            var businessId = Guid.Parse(_currentUserService.GetCompany()!);
            var members = await _chatService.GetBusinessMembersAsync(businessId, userId);
            return Ok(members);
        }

        [HttpGet("history/{otherUserId}")]
        public async Task<IActionResult> GetHistory(
            Guid otherUserId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 30)
        {
            var userId = Guid.Parse(_currentUserService.GetUserId()!);
            var businessId = Guid.Parse(_currentUserService.GetCompany()!);
            var messages = await _chatService.GetHistoryAsync(userId, otherUserId, businessId, page, pageSize);
            return Ok(messages);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = Guid.Parse(_currentUserService.GetUserId()!);
            var businessId = Guid.Parse(_currentUserService.GetCompany()!);
            var count = await _chatService.GetUnreadCountAsync(userId, businessId);
            return Ok(new { count });
        }
    }
}