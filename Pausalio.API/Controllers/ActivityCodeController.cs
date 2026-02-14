using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.ActivityCode;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityCodeController : ControllerBase
    {
        private readonly IActivityCodeService _service;
        private readonly ILogger<ActivityCodeController> _logger;
        private readonly ILocalizationHelper _localizationHelper;

        public ActivityCodeController(
            IActivityCodeService service,
            ILogger<ActivityCodeController> logger,
            ILocalizationHelper localizationHelper)
        {
            _service = service;
            _logger = logger;
            _localizationHelper = localizationHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound(new { success = false, message = _localizationHelper.ActivityCodeNotFound });

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        
        public async Task<IActionResult> Create(AddActivityCodeDto dto)
        {
            try
            {
                await _service.CreateAsync(dto);
                return Ok(new { success = true, message = _localizationHelper.ActivityCodeCreatedSuccessfully });
            }
            catch (Exception ex)
            {
                return BadRequest( new { success = false, message =  ex.Message });
            }
           
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateActivityCodeDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok(new { success = true, message = _localizationHelper.ActivityCodeUpdatedSuccessfully });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { success = true, message = _localizationHelper.ActivityCodeDeletedSuccessfully });
            }
            catch (Exception ex)
            {
                return BadRequest(new {success = false, message = ex.Message});
            }
            
        }
    }
}