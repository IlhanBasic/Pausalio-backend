using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pausalio.Application.DTOs.AIAssistant;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        /// Pristupačno na: POST /api/AIAssistant/send ili POST /api/AIAssistant (radi unazadne kompatibilnosti)
        /// </summary>
        [HttpPost("send")]
        [HttpPost]
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

        /// <summary>
        /// Vraća listu svih aktivnih razgovora trenutnog korisnika.
        /// Pristupačno na: GET /api/AIAssistant/conversations
        /// </summary>
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var conversations = await _aiAssistentService.GetConversationsAsync();
                return Ok(conversations);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access while fetching conversations");
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching conversations");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Vraća sve poruke za određenu konverziju.
        /// Pristupačno na: GET /api/AIAssistant/conversations/{conversationId}/messages
        /// </summary>
        [HttpGet("conversations/{conversationId:guid}/messages")]
        public async Task<IActionResult> GetConversationMessages([FromRoute] Guid conversationId)
        {
            try
            {
                var messages = await _aiAssistentService.GetConversationMessagesAsync(conversationId);
                return Ok(messages);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Conversation not found: {ConversationId}", conversationId);
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access while fetching conversation messages");
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching messages for conversation {ConversationId}", conversationId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Briše (logički) odabranu konverziju.
        /// Pristupačno na: DELETE /api/AIAssistant/conversations/{conversationId}
        /// </summary>
        [HttpDelete("conversations/{conversationId:guid}")]
        public async Task<IActionResult> DeleteConversation([FromRoute] Guid conversationId)
        {
            try
            {
                await _aiAssistentService.DeleteConversationAsync(conversationId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Conversation not found for deletion: {ConversationId}", conversationId);
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access while deleting conversation");
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation {ConversationId}", conversationId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }
    }
}