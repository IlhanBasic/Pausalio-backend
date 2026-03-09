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

        /// <summary>
        /// Služi za dohvaćanje svih podsjetnika za trenutno prijavljenog korisnika. Vraća listu podsjetnika u obliku JSON objekta.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Služi za dohvaćanje pojedinačnog podsjetnika na temelju njegovog jedinstvenog identifikatora (ID). Ako podsjetnik s navedenim ID-om ne postoji, vraća se HTTP status 404 Not Found s odgovarajućom porukom. Ako podsjetnik postoji, vraća se njegov detaljni prikaz u obliku JSON objekta.
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { success = false, message = _localizationHelper.ReminderNotFound });

            return Ok(result);
        }

        /// <summary>
        /// Služi za kreiranje novog podsjetnika. Prima podatke o podsjetniku u obliku JSON objekta (AddReminderDto) i pokušava ga spremiti u bazu podataka. Ako je operacija uspješna, vraća se HTTP status 200 OK s porukom o uspješnom kreiranju. Ako dođe do greške tijekom kreiranja (npr. zbog neispravnih podataka), vraća se HTTP status 404 Not Found s odgovarajućom porukom o grešci.
        /// </summary>
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

        /// <summary>
        /// Služi za ažuriranje postojećeg podsjetnika. Prima jedinstveni identifikator (ID) podsjetnika koji se želi ažurirati i nove podatke o podsjetniku u obliku JSON objekta (UpdateReminderDto). Ako je operacija uspješna, vraća se HTTP status 200 OK s porukom o uspješnom ažuriranju. Ako dođe do greške tijekom ažuriranja (npr. ako podsjetnik s navedenim ID-om ne postoji), vraća se HTTP status 404 Not Found s odgovarajućom porukom o grešci.
        /// </summary>
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

        /// <summary>
        /// Služi za označavanje podsjetnika kao dovršenog. Prima jedinstveni identifikator (ID) podsjetnika koji se želi označiti kao dovršen. Ako je operacija uspješna, vraća se HTTP status 200 OK s porukom o uspješnom označavanju kao dovršenog. Ako dođe do greške tijekom označavanja (npr. ako podsjetnik s navedenim ID-om ne postoji), vraća se HTTP status 404 Not Found s odgovarajućom porukom o grešci.
        /// </summary>
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

        /// <summary>
        /// Služi za brisanje podsjetnika. Prima jedinstveni identifikator (ID) podsjetnika koji se želi obrisati. Ako je operacija uspješna, vraća se HTTP status 200 OK s porukom o uspješnom brisanju. Ako dođe do greške tijekom brisanja (npr. ako podsjetnik s navedenim ID-om ne postoji), vraća se HTTP status 404 Not Found s odgovarajućom porukom o grešci.
        /// </summary>
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