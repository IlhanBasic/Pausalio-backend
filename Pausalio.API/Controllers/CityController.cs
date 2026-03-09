using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.City;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly ICityService _cityService;
        private readonly ILogger<CityController> _logger;
        private readonly ILocalizationHelper _localizationHelper;

        public CityController(
            ICityService cityService,
            ILogger<CityController> logger,
            ILocalizationHelper localizationHelper)
        {
            _cityService = cityService;
            _logger = logger;
            _localizationHelper = localizationHelper;
        }

        /// <summary>
        /// Služi za dohvaćanje svih gradova. Ova metoda je javno dostupna i ne zahtijeva autentifikaciju.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cities = await _cityService.GetAllCities();
            return Ok(cities);
        }

        /// <summary>
        /// Služi za dohvaćanje detalja o određenom gradu na temelju njegovog ID-a. Ova metoda je javno dostupna i ne zahtijeva autentifikaciju.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var city = await _cityService.GetCityById(id);

            if (city == null)
                return NotFound(new { success = true , message  = _localizationHelper.CityNotFound});

            return Ok(city);
        }

        /// <summary>
        /// Služi za kreiranje novog grada. Ova metoda je ograničena samo na korisnike s ulogom "Admin". Potrebno je poslati JSON objekt koji sadrži naziv i poštanski broj grada.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddCityDto dto)
        {
            try
            {
                await _cityService.CreateCity(dto);
                return Ok(new { success = true, message = _localizationHelper.CityCreatedSuccessfully });
            }
            catch(Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
           
        }

        /// <summary>
        /// Služi za ažuriranje postojećeg grada na temelju njegovog ID-a. Ova metoda je ograničena samo na korisnike s ulogom "Admin". Potrebno je poslati JSON objekt koji sadrži novi naziv i/ili poštanski broj grada.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCityDto dto)
        {
            try
            {
                await _cityService.UpdateCity(id, dto);
                return Ok(new { success = true, message = _localizationHelper.CityUpdatedSuccessfully });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Služi za brisanje postojećeg grada na temelju njegovog ID-a. Ova metoda je ograničena samo na korisnike s ulogom "Admin". Ako grad ne postoji, vraća se poruka o neuspjehu. Ako je brisanje uspješno, vraća se poruka o uspjehu.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _cityService.DeleteCity(id);
                return Ok(new { success = true, message = _localizationHelper.CityDeletedSuccessfully });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }
    }
}