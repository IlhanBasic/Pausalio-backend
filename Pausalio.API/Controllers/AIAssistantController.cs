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
        private readonly ILogger<AIAssistantController> _logger;

        public AIAssistantController(
            ILocalizationHelper localizationHelper,
            IAIAssistantService aIAssistentService,
            ILogger<AIAssistantController> logger)
        {
            _localizationHelper = localizationHelper;
            _aiAssistentService = aIAssistentService;
            _logger = logger;
        }

        /// <summary>
        /// Služi za slanje poruka AI asistentu i dobijanje odgovora.
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromBody] UserChatMessage message)
        {
            try
            {
                var response = await _aiAssistentService.SendMessageAsync(message);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation in AI Assistant");

                return BadRequest(new
                {
                    ExceptionType = ex.GetType().FullName,
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in AI Assistant");

                return Unauthorized(new
                {
                    ExceptionType = ex.GetType().FullName,
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in AI Assistant");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    ExceptionType = ex.GetType().FullName,
                    Message = ex.Message,
                    InnerException = ex.InnerException?.ToString(),
                    StackTrace = ex.StackTrace
                });
            }
        }
    }
}