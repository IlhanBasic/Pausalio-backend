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

        /// <summary>
        /// Služi za dohvaćanje svih dokumenata. Ova metoda je ograničena na korisnike s ulogom "RegularUser". Vraća listu svih dokumenata u sustavu.
        /// </summary>
        [HttpGet]
        [Authorize(Roles ="RegularUser")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Služi za dohvaćanje pojedinačnog dokumenta po njegovom jedinstvenom identifikatoru (ID). Ova metoda je ograničena na korisnike s ulogom "RegularUser". Ako dokument s navedenim ID-om ne postoji, vraća se poruka o neuspjehu. Inače, vraća se traženi dokument.
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound(new { success = true, message = _localizationHelper.DocumentNotFound });

            return Ok(result);
        }

        /// <summary>
        /// Služi za kreiranje novog dokumenta. Ova metoda je ograničena na korisnike s ulogom "RegularUser". Prima podatke o dokumentu putem DTO objekta i pokušava kreirati novi dokument. Ako dođe do greške tijekom kreiranja, vraća se poruka o neuspjehu. Inače, vraća se poruka o uspješnom kreiranju dokumenta.
        /// </summary>
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

        /// <summary>
        /// Služi za ažuriranje postojećeg dokumenta. Ova metoda je ograničena na korisnike s ulogom "RegularUser". Prima ID dokumenta koji se želi ažurirati i nove podatke o dokumentu putem DTO objekta. Pokušava ažurirati dokument s navedenim ID-om. Ako dođe do greške tijekom ažuriranja, vraća se poruka o neuspjehu. Inače, vraća se poruka o uspješnom ažuriranju dokumenta.
        /// </summary>
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

        /// <summary>
        /// Služi za brisanje postojećeg dokumenta. Ova metoda je ograničena na korisnike s ulogom "RegularUser". Prima ID dokumenta koji se želi obrisati. Pokušava obrisati dokument s navedenim ID-om. Ako dođe do greške tijekom brisanja, vraća se poruka o neuspjehu. Inače, vraća se poruka o uspješnom brisanju dokumenta.
        /// </summary>
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