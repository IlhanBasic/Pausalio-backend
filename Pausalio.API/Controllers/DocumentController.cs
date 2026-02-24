using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.Document;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _service;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly IUploadFileService _uploadFileService;
        public DocumentController(
            IDocumentService service,
            ILocalizationHelper localizationHelper,
            IUploadFileService uploadFileService)
        {
            _service = service;
            _localizationHelper = localizationHelper;
            _uploadFileService = uploadFileService;
        }

        [HttpGet]
        [Authorize(Roles ="RegularUser")]
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
                return NotFound(new { success = true, message = _localizationHelper.DocumentNotFound });

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> Create(AddDocumentDto dto)
        {
            try
            {
                await _service.CreateAsync(dto);
                return Ok(new { success = true, message = _localizationHelper.DocumentCreatedSuccessfully });
            }
            catch (Exception ex)
            {
                return BadRequest( new { success = false, message = ex.Message });
            }
           
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> Update(Guid id, UpdateDocumentDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok(new { success = true, message = _localizationHelper.DocumentUpdatedSuccessfully });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = true, message = ex.Message });
            }
            
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var document = await _service.GetByIdAsync(id);
                if (document == null)
                    return NotFound(new { success = false, message = _localizationHelper.DocumentNotFound });
                if(!string.IsNullOrEmpty(document.FilePath))
                {
                    await _uploadFileService.DeleteFileAsync(document.FilePath);
                }
                await _service.DeleteAsync(id);
                return Ok(new { success = true, message = _localizationHelper.DocumentDeletedSuccessfully });
            }
            catch(Exception ex)
            {
                return NotFound(new { success =  false, message = ex.Message });
            }
            
        }
    }
}