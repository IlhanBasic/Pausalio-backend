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

        /// <summary>
        /// Služi za dohvaćanje članova poslovnog računa s kojima korisnik može komunicirati putem chata. Vraća listu članova poslovnog računa, uključujući njihove osnovne informacije (npr. ime, email) i status (npr. online/offline). Ova metoda omogućava korisniku da vidi tko je dostupan za chat unutar poslovnog računa.
        /// </summary>
        [HttpGet("members")]
        public async Task<IActionResult> GetBusinessMembers()
        {
            var userId = Guid.Parse(_currentUserService.GetUserId()!);
            var businessId = Guid.Parse(_currentUserService.GetCompany()!);
            var members = await _chatService.GetBusinessMembersAsync(businessId, userId);
            return Ok(members);
        }

        /// <summary>
        /// Služi za dohvaćanje povijesti poruka između trenutnog korisnika i drugog korisnika unutar istog poslovnog računa. Vraća paginiranu listu poruka, uključujući informacije o pošiljatelju, primatelju, sadržaju poruke i vremenu slanja. Ova metoda omogućava korisniku da vidi prethodne razgovore s određenim članom poslovnog računa.
        /// </summary>
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

        /// <summary>
        /// Služi za dohvaćanje broja nepročitanih poruka za trenutnog korisnika unutar poslovnog računa. Vraća broj nepročitanih poruka koje korisnik ima od drugih članova poslovnog računa. Ova metoda omogućava korisniku da vidi koliko novih poruka ima bez potrebe da otvara svaki razgovor pojedinačno.
        /// </summary>
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