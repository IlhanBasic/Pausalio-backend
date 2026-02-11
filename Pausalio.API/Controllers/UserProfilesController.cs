using Microsoft.AspNetCore.Mvc;
using Pausalio.Application.DTOs.UserProfile;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Localization;
using Serilog;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly ILocalizationHelper _localizationHelper;

        public UserProfilesController(
            IUserProfileService userProfileService,
            ILocalizationHelper localizationHelper)
        {
            _userProfileService = userProfileService;
            _localizationHelper = localizationHelper;
        }

        [HttpPost]
        public async Task<ActionResult<UserProfileToReturnDto>> CreateUserProfile([FromBody] AddUserProfileDto dto)
        {
            // FluentValidation automatski validira DTO jer je [ApiController] i AddFluentValidation registrovan
            // Ako DTO nije validan, ASP.NET Core automatski vraća 400 BadRequest sa detaljima ModelState

            try
            {
                var userProfile = await _userProfileService.CreateUserProfile(dto);
                if (userProfile == null)
                {
                    Log.Warning("Failed to create user profile for Email: {Email}", dto.Email);
                    return BadRequest(_localizationHelper.UserProfileCreationFailed);
                }

                Log.Information("User profile created successfully. Email: {Email}", dto.Email);
                return Ok(userProfile);
            }
            catch (ArgumentNullException ex)
            {
                Log.Warning(ex, "Null argument when creating user profile. Email: {Email}", dto.Email);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error when creating user profile. Email: {Email}", dto.Email);
                return StatusCode(500, _localizationHelper.ServerError);
            }
        }

        
    }
}
