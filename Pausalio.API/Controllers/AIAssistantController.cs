using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.AIAssistant;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIAssistantController : ControllerBase
    {
        private readonly ILocalizationHelper _localizationHelper;
        private readonly IAIAssistantService _aiAssistentService;
        public AIAssistantController(ILocalizationHelper localizationHelper, IAIAssistantService aIAssistentService)
        {
            _localizationHelper = localizationHelper;
            _aiAssistentService = aIAssistentService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromBody] UserChatMessage message)
        {
            var response = await _aiAssistentService.SendMessageAsync(message);
            return Ok(response);
        }
    }
}
