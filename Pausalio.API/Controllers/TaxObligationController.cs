using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.TaxObligation;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "RegularUser")]
    public class TaxObligationController : ControllerBase
    {
        private readonly ITaxObligationService _taxObligationService;
        private readonly ILocalizationHelper _localizationHelper;

        public TaxObligationController(
            ITaxObligationService taxObligationService,
            ILocalizationHelper localizationHelper)
        {
            _taxObligationService = taxObligationService;
            _localizationHelper = localizationHelper;
        }

        /// <summary>
        /// Vraća sve poreske obaveze trenutne firme
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var obligations = await _taxObligationService.GetAllAsync();
                return Ok(new { success = true, data = obligations, count = obligations.Count() });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Vraća obaveze za određenu godinu
        /// </summary>
        [HttpGet("year/{year:int}")]
        public async Task<IActionResult> GetByYear(int year)
        {
            try
            {
                var obligations = await _taxObligationService.GetByYearAsync(year);
                return Ok(new { success = true, data = obligations, count = obligations.Count() });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Vraća obaveze za određeni mesec i godinu
        /// </summary>
        [HttpGet("year/{year:int}/month/{month:int}")]
        public async Task<IActionResult> GetByYearAndMonth(int year, int month)
        {
            try
            {
                var obligation = await _taxObligationService.GetByYearAndMonthAsync(year, month);

                if (obligation == null)
                    return NotFound(new { success = false, message = _localizationHelper.TaxObligationNotFound });

                return Ok(new { success = true, data = obligation });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Vraća obaveze po statusu
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(TaxObligationStatus status)
        {
            try
            {
                var obligations = await _taxObligationService.GetByStatusAsync(status);
                return Ok(new { success = true, data = obligations, count = obligations.Count() });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Vraća jednu poresku obavezu po ID-ju
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var obligation = await _taxObligationService.GetByIdAsync(id);

                if (obligation == null)
                    return NotFound(new { success = false, message = _localizationHelper.TaxObligationNotFound });

                return Ok(new { success = true, data = obligation });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 🆕 AUTOMATSKO GENERISANJE - Generiše poreske obaveze za godinu
        /// </summary>
        [HttpPost("generate")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GenerateAnnualTaxObligations([FromBody] GenerateTaxObligationsDto dto)
        {
            try
            {
                var generated = await _taxObligationService.GenerateAnnualObligationsAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = _localizationHelper.TaxObligationsGeneratedSuccessfully,
                    data = generated,
                    count = generated.Count()
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 🆕 RUČNO DODAVANJE - Kreira pojedinačnu poresku obavezu
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Create([FromBody] AddTaxObligationDto dto)
        {
            try
            {
                var created = await _taxObligationService.CreateAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = _localizationHelper.TaxObligationCreatedSuccessfully,
                    data = created
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Ažurira poresku obavezu
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxObligationDto dto)
        {
            try
            {
                await _taxObligationService.UpdateAsync(id, dto);
                return Ok(new { success = true, message = _localizationHelper.TaxObligationUpdatedSuccessfully });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Briše poresku obavezu
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _taxObligationService.DeleteAsync(id);
                return Ok(new { success = true, message = _localizationHelper.TaxObligationDeletedSuccessfully });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Označi obavezu kao plaćenu
        /// </summary>
        [HttpPatch("{id:guid}/mark-paid")]
        [Authorize(Roles = "Owner, Assistant")]
        public async Task<IActionResult> MarkAsPaid(Guid id)
        {
            try
            {
                await _taxObligationService.MarkAsPaidAsync(id);
                return Ok(new { success = true, message = _localizationHelper.TaxObligationMarkedAsPaid });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Vraća summary (pregled obaveza)
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int? year)
        {
            try
            {
                var summary = await _taxObligationService.GetSummaryAsync(year);
                return Ok(new { success = true, data = summary });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}