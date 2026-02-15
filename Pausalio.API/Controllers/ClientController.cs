using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.Client;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "RegularUser")]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILocalizationHelper _localizationHelper;

        public ClientController(
            IClientService clientService,
            ILocalizationHelper localizationHelper)
        {
            _clientService = clientService;
            _localizationHelper = localizationHelper;
        }

        /// <summary>
        /// Vraća sve klijente trenutne firme
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var clients = await _clientService.GetAllAsync();
                return Ok(new { success = true, data = clients, count = clients.Count() });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Vraća klijente po tipu (Domestic, Foreign, Individual)
        /// </summary>
        [HttpGet("type/{clientType}")]
        public async Task<IActionResult> GetByType(ClientType clientType)
        {
            try
            {
                var clients = await _clientService.GetByTypeAsync(clientType);
                return Ok(new { success = true, data = clients, count = clients.Count() });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Vraća jednog klijenta po ID-ju
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var client = await _clientService.GetByIdAsync(id);

                if (client == null)
                    return NotFound(new { success = false, message = _localizationHelper.ClientNotFound });

                return Ok(new { success = true, data = client });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Kreira novog klijenta za trenutnu firmu
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Create([FromBody] AddClientDto dto)
        {
            try
            {
                await _clientService.CreateAsync(dto);
                return Ok(new { success = true, message = _localizationHelper.ClientCreatedSuccessfully });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Ažurira postojećeg klijenta
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientDto dto)
        {
            try
            {
                await _clientService.UpdateAsync(id, dto);
                return Ok(new { success = true, message = _localizationHelper.ClientUpdatedSuccessfully });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Soft delete - deaktivira klijenta
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _clientService.DeleteAsync(id);
                return Ok(new { success = true, message = _localizationHelper.ClientDeletedSuccessfully });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Aktivira deaktiviranog klijenta
        /// </summary>
        [HttpPatch("{id:guid}/activate")]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Activate(Guid id)
        {
            try
            {
                await _clientService.ActivateAsync(id);
                return Ok(new { success = true, message = _localizationHelper.ClientActivatedSuccessfully });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}