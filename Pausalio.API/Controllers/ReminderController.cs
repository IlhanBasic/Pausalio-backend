using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.Reminder;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReminderController : ControllerBase
    {
        private readonly IReminderService _service;
        private readonly ILocalizationHelper _localizationHelper;

        public ReminderController(IReminderService service, ILocalizationHelper localizationHelper)
        {
            _service = service;
            _localizationHelper = localizationHelper;
        }

        [HttpGet]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { success = false, message = _localizationHelper.ReminderNotFound });

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> Create(AddReminderDto dto)
        {
            try
            {
                await _service.CreateAsync(dto);
                return Ok(new { success = true, message = _localizationHelper.ReminderCreatedSuccessfully });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
          
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> Update(Guid id, UpdateReminderDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok(new { success = true, message = _localizationHelper.ReminderUpdatedSuccessfully });
            }
            catch (Exception ex)
            {
                return NotFound( new { success = false, message = ex.Message });
            }
            
        }

        [HttpPatch("{id:guid}/complete")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> MarkCompleted(Guid id)
        {
            try
            {
                await _service.MarkCompletedAsync(id);
                return Ok(new { success = true, message = _localizationHelper.ReminderMarkedCompleted });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
           
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { success = true, message = _localizationHelper.ReminderDeletedSuccessfully });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            
        }
    }
}