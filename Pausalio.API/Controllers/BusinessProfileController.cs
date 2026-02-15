using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.BusinessProfile;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessProfileController : ControllerBase
    {
        private readonly IBusinessProfileService _businessProfileService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILocalizationHelper _localizationHelper;

        public BusinessProfileController(
            IBusinessProfileService businessProfileService,
            ICurrentUserService currentUserService,
            ILocalizationHelper localizationHelper)
        {
            _businessProfileService = businessProfileService;
            _currentUserService = currentUserService;
            _localizationHelper = localizationHelper;
        }

        /// <summary>
        /// Vraća kompaniju trenutno ulogovanog korisnika (Owner ili Assistant)
        /// Koristi X-Business-Context header za context switching
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> GetUserCompany()
        {
            try
            {
                var companyIdString = _currentUserService.GetCompany();

                if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                    return BadRequest(new { success = false, message = _localizationHelper.InvalidCompanyId });

                var company = await _businessProfileService.GetByIdAsync(companyId);

                if (company == null)
                    return NotFound(new { success = false, message = _localizationHelper.BusinesProfileNotFound });

                return Ok(new { success = true, data = company });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Vraća SVE firme trenutno ulogovanog korisnika (za asistente sa više firmi)
        /// </summary>
        [HttpGet("user/all")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> GetUserCompanies()
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
                    return Unauthorized(new { success = false, message = _localizationHelper.Unauthorized });

                var companies = await _businessProfileService.GetUserBusinessProfilesAsync(userGuid);

                return Ok(new { success = true, data = companies });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Ažurira kompaniju - samo Owner može da izmeni
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateCompany([FromBody] UpdateBusinessProfileDto dto)
        {
            try
            {
                var companyIdString = _currentUserService.GetCompany();

                if (companyIdString == null || !Guid.TryParse(companyIdString, out Guid companyId))
                    return BadRequest(new { success = false, message = _localizationHelper.InvalidCompanyId });

                await _businessProfileService.UpdateAsync(companyId, dto);

                return Ok(new { success = true, message = _localizationHelper.CompanyUpdatedSuccessfully });
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
        /// Admin endpoint - vraća SVE kompanije u sistemu
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCompanies()
        {
            try
            {
                var companies = await _businessProfileService.GetAllAsync();

                return Ok(new { success = true, data = companies, count = companies.Count() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Admin endpoint - vraća jednu kompaniju po ID-ju
        /// </summary>
        [HttpGet("admin/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCompanyById(Guid id)
        {
            try
            {
                var company = await _businessProfileService.GetByIdAsync(id);

                if (company == null)
                    return NotFound(new { success = false, message = _localizationHelper.BusinesProfileNotFound });

                return Ok(new { success = true, data = company });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Admin endpoint - deaktivira kompaniju (soft delete)
        /// </summary>
        [HttpPatch("admin/{id:guid}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateCompany(Guid id)
        {
            try
            {
                await _businessProfileService.DeactivateAsync(id);

                return Ok(new { success = true, message = _localizationHelper.CompanyDeactivatedSuccessfully });
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
        /// Admin endpoint - aktivira kompaniju
        /// </summary>
        [HttpPatch("admin/{id:guid}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateCompany(Guid id)
        {
            try
            {
                await _businessProfileService.ActivateAsync(id);

                return Ok(new { success = true, message = _localizationHelper.CompanyActivatedSuccessfully });
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
    }
}