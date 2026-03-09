using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.BankAccount;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService _service;
        private readonly ILocalizationHelper _localizationHelper;

        public BankAccountController(IBankAccountService service, ILocalizationHelper localizationHelper)
        {
            _service = service;
            _localizationHelper = localizationHelper;
        }

        /// <summary>
        /// Služi za dobijanje svih bankovnih računa povezanih sa korisnikom.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Služi za dobijanje detalja o određenom bankovnom računu na osnovu njegovog ID-a. Ako račun ne postoji, vraća se 404 Not Found sa odgovarajućom porukom.
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { success = false, message = _localizationHelper.BankAccountNotFound });

            return Ok(result);
        }

        /// <summary>
        /// Služi za kreiranje novog bankovnog računa. Očekuje se da će klijent poslati JSON objekt koji sadrži potrebne informacije o bankovnom računu. Ako je kreiranje uspješno, vraća se 200 OK sa porukom o uspjehu. Ako dođe do greške (npr. nedostajuća polja, neispravan format), vraća se 400 Bad Request sa detaljima o grešci.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> Create(AddBankAccountDto dto)
        {
            try
            {
                await _service.CreateAsync(dto);
                return Ok(new { success = true, message = _localizationHelper.BankAccountCreatedSuccessfully });
            }
            catch(Exception Ex)
            {
                return BadRequest(new { success = false, message = Ex.Message});
            }
            
        }

        /// <summary>
        /// Služi za ažuriranje postojećeg bankovnog računa. Očekuje se da će klijent poslati JSON objekt sa ažuriranim informacijama o bankovnom računu. Ako je ažuriranje uspješno, vraća se 200 OK sa porukom o uspjehu. Ako račun ne postoji, vraća se 404 Not Found sa odgovarajućom porukom. Ako dođe do greške (npr. nedostajuća polja, neispravan format), vraća se 400 Bad Request sa detaljima o grešci.
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> Update(Guid id, UpdateBankAccountDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok(new { success = true, message = _localizationHelper.BankAccountUpdatedSuccessfully });
            }
            catch (Exception ex)
            {
                return NotFound(new {success = false, message =  ex.Message});
            }
           
        }

        /// <summary>
        /// Služi za brisanje postojećeg bankovnog računa. Ako je brisanje uspješno, vraća se 200 OK sa porukom o uspjehu. Ako račun ne postoji, vraća se 404 Not Found sa odgovarajućom porukom.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { success = true, message = _localizationHelper.BankAccountDeletedSuccessfully });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
           
        }
    }
}