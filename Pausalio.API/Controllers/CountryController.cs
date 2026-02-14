using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.Country;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly ICountryService _countryService;
        private readonly ILogger<CountryController> _logger;
        private readonly ILocalizationHelper _localizationHelper;

        public CountryController(
            ICountryService countryService,
            ILogger<CountryController> logger,
            ILocalizationHelper localizationHelper)
        {
            _countryService = countryService;
            _logger = logger;
            _localizationHelper = localizationHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var countries = await _countryService.GetAllCountries();
            return Ok(countries);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var country = await _countryService.GetCountryById(id);

            if (country == null)
                return NotFound(new { success = false , essage = _localizationHelper.CountryNotFound });

            return Ok(country);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddCountryDto dto)
        {
            try
            {
                await _countryService.CreateCountry(dto);
                return Ok(new { success = true, message = _localizationHelper.CountryCreatedSuccessfully });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCountyDto dto)
        {
            try
            {
                await _countryService.UpdateCountry(id, dto);
                return Ok(new { success = true, message = _localizationHelper.CountryUpdatedSuccessfully });
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
                await _countryService.DeleteCountry(id);
                return Ok(new { success = true, message = _localizationHelper.CountryDeletedSuccessfully });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            
        }
    }
}