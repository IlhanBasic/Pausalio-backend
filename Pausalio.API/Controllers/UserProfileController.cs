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

        [HttpPut("id:{guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateUserProfileDto dto)
        {
            try
            {
                var user = await _userProfileService.UpdateProfile(id,dto);
                return Ok(new {success = true, message = _localizationHelper.UserUpdatedSuccessfully});
            }
            catch (Exception ex)
            {
                return BadRequest(new {success = false, message = ex.Message});
            }
        }
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var contextUser = _currentUserService.GetEmail();
            if (contextUser == null)
                return BadRequest(new { success = false, message = _localizationHelper.Unauthorized });

            var user = await _userProfileService.GetByEmailAsync(contextUser);
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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userProfileService.GetAllUsers();
            return Ok(users);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                await _userProfileService.DeleteUser(id);
                return Ok(new { success = true, message = _localizationHelper.UserDeletedSuccessfully});
            }
            catch (Exception ex)
            {
                return NotFound(new {success = false, message = ex.Message});
            }
        }
    }
}
