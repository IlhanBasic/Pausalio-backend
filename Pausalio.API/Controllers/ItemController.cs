using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.Item;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _service;
        private readonly ILocalizationHelper _localizationHelper;

        public ItemController(IItemService service, ILocalizationHelper localizationHelper)
        {
            _service = service;
            _localizationHelper = localizationHelper;
        }

        /// <summary>
        /// Služi za dobijanje svih itema. Vraća listu itema ili grešku ako dobijanje nije uspelo.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Služi za dobijanje itema po ID-u. Vraća item ako je pronađen ili grešku ako nije pronađen.
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { success = true, message = _localizationHelper.ItemNotFound });

            return Ok(result);
        }

        /// <summary>
        /// Služi za kreiranje novog itema. Prima AddItemDto objekat i vraća poruku o uspehu ili grešku ako kreiranje nije uspelo.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> Create(AddItemDto dto)
        {
            try
            {
                await _service.CreateAsync(dto);
                return Ok(new { success = true, message = _localizationHelper.ItemCreatedSuccessfully });
            }
            catch (Exception ex)
            {
                return BadRequest(new {success = false, message =ex.Message});
            }
        }

        /// <summary>
        /// Služi za ažuriranje postojećeg itema. Prima ID itema i UpdateItemDto objekat, vraća poruku o uspehu ili grešku ako ažuriranje nije uspelo.
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> Update(Guid id, UpdateItemDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok(new { success = true, message = _localizationHelper.ItemUpdatedSuccessfully });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Služi za brisanje itema. Prima ID itema i vraća poruku o uspehu ili grešku ako brisanje nije uspelo.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { success = true, message = _localizationHelper.ItemDeletedSuccessfully });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
          
        }
    }
}