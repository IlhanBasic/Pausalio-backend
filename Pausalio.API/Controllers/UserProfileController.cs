using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.UserProfile;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly ICurrentUserService _currentUserService;
        public UserProfileController(IUserProfileService userProfileService, ILocalizationHelper localizationHelper, ICurrentUserService currentUserService)
        {
            _userProfileService = userProfileService;
            _localizationHelper = localizationHelper;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Služi za ažuriranje profila korisnika. Korisnik može ažurirati samo svoj profil, dok administratori mogu ažurirati profile svih korisnika.
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateUserProfileDto dto)
        {
            try
            {
                var user = await _userProfileService.UpdateProfile(id, dto);
                return Ok(new { success = true, message = _localizationHelper.UserUpdatedSuccessfully });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Služi za dohvaćanje profila trenutno prijavljenog korisnika. Korisnik može dohvatiti samo svoj profil, dok administratori mogu dohvatiti profile svih korisnika.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var contextUser = _currentUserService.GetEmail();
            if (contextUser == null)
                return BadRequest(new { success = false, message = _localizationHelper.Unauthorized });

            var user = await _userProfileService.GetByEmailAsync(contextUser);
            Console.WriteLine($"=== GET ME === ProfilePicture: {user?.ProfilePicture ?? "NULL"}");

            if (user == null)
                return NotFound(new { success = false, message = _localizationHelper.UserNotFound });

            if (user.Role != UserRole.Admin)
            {
                var currentCompanyString = _currentUserService.GetCompany();
                if (currentCompanyString == null)
                    return BadRequest(new { success = false, message = _localizationHelper.InvalidCompanyId });

                if (!Guid.TryParse(currentCompanyString.ToString(), out Guid companyGuid))
                    return BadRequest(new { success = false, message = _localizationHelper.InvalidCompanyId });

                var company = await _userProfileService.GetCompanyById(companyGuid);
                if (company == null)
                    return BadRequest(new { success = false, message = _localizationHelper.UserCompanyNotFound });

                return Ok(new ProfileToReturnDto
                {
                    UserProfile = user,
                    BusinessProfile = company
                });
            }

            return Ok(new ProfileToReturnDto { UserProfile = user });
        }

        /// <summary>
        /// Služi za dohvaćanje svih korisnika. Samo administratori imaju pristup ovom endpointu, dok redoviti korisnici nemaju dozvolu za dohvaćanje popisa svih korisnika.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userProfileService.GetAllUsers();
            return Ok(users);
        }

        /// <summary>
        /// Služi za brisanje korisnika na temelju njegovog jedinstvenog identifikatora (ID). Samo administratori imaju pristup ovom endpointu, dok redoviti korisnici nemaju dozvolu za brisanje korisnika. Ako korisnik s navedenim ID-om ne postoji, vraća se HTTP status 404 Not Found s odgovarajućom porukom. Ako je operacija brisanja uspješna, vraća se HTTP status 200 OK s porukom o uspješnom brisanju korisnika.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                await _userProfileService.DeleteUser(id);
                return Ok(new { success = true, message = _localizationHelper.UserDeletedSuccessfully });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Služi za aktivaciju korisnika na temelju njegovog jedinstvenog identifikatora (ID). Samo administratori imaju pristup ovom endpointu, dok redoviti korisnici nemaju dozvolu za aktivaciju korisnika. Ako korisnik s navedenim ID-om ne postoji, vraća se HTTP status 404 Not Found s odgovarajućom porukom. Ako je operacija aktivacije uspješna, vraća se HTTP status 200 OK s porukom o uspješnoj aktivaciji korisnika.
        /// </summary>
        [HttpPatch("{id:guid}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateUser(Guid id)
        {
            try
            {
                await _userProfileService.SetUserActiveStatus(id, true);
                return Ok(new { success = true, message = _localizationHelper.UserActivatedSuccessfully });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Služi za deaktivaciju korisnika na temelju njegovog jedinstvenog identifikatora (ID). Samo administratori imaju pristup ovom endpointu, dok redoviti korisnici nemaju dozvolu za deaktivaciju korisnika. Ako korisnik s navedenim ID-om ne postoji, vraća se HTTP status 404 Not Found s odgovarajućom porukom. Ako je operacija deaktivacije uspješna, vraća se HTTP status 200 OK s porukom o uspješnoj deaktivaciji korisnika.
        /// </summary>
        [HttpPatch("{id:guid}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateUser(Guid id)
        {
            try
            {
                await _userProfileService.SetUserActiveStatus(id, false);
                return Ok(new { success = true, message = _localizationHelper.UserDeactivatedSuccessfully });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }
    }
}
