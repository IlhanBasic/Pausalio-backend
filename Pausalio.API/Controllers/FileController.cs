using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IUploadFileService _uploadFileService;
        private readonly ILocalizationHelper _localizationHelper;
        public FileController(IUploadFileService uploadFileService, ILocalizationHelper localizationHelper)
        {
            _uploadFileService = uploadFileService;
            _localizationHelper = localizationHelper;
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteFile([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest(new { success = false, message = _localizationHelper.UrlIsRequired});

            try
            {
                await _uploadFileService.DeleteFileAsync(url);
                return Ok(new { success = true, message = _localizationHelper.FileDeletedSuccessfully });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = _localizationHelper.FileDeleteFailed, detail = ex.Message });
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = _localizationHelper.FileIsEmptyOrNotProvided });

            var allowedContentTypes = new[]
            {
				// Images
				"image/jpeg",
                "image/png",
                "image/gif",
                "image/bmp",
                "image/webp",

				// PDF
				"application/pdf",

				// Microsoft Office
				"application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",

				// Text files
				"text/plain",
                "text/csv"
            };

            if (!Array.Exists(allowedContentTypes, ct => ct.Equals(file.ContentType, StringComparison.OrdinalIgnoreCase)))
                return BadRequest(new { success = false, message = _localizationHelper.UnsupportedFileType });

            try
            {
                var url = await _uploadFileService.UploadFileAsync(file.OpenReadStream(), file.FileName, file.ContentType);
                return Ok(new { success = true, message = _localizationHelper.FileUploadedSuccessfully, url });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = _localizationHelper.FileUploadFailed, detail = ex.Message });
            }
        }
    }
}
