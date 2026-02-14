using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pausalio.Application.DTOs.BusinessInvite;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Configuration;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessInviteController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IBusinessInviteService _businessInviteService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly IOptions<UrlSettings> _urlSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserProfileService _userProfileService;
        public BusinessInviteController(IUserProfileService userProfileService, ICurrentUserService currentUserService, IHttpContextAccessor httpContextAccessor, IOptions<UrlSettings> urlSettings, ILocalizationHelper localizationHelper, IEmailService emailService, IEmailTemplateService emailTemplateService, IBusinessInviteService businessInviteService)
        {
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
            _urlSettings = urlSettings;
            _localizationHelper = localizationHelper;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _businessInviteService = businessInviteService;
            _userProfileService = userProfileService;
        }
        [Authorize(Roles = "Owner")]
        [HttpPost]
        public async Task<IActionResult> SendInvite([FromBody] AddBusinessInviteDto dto)
        {
            var userEmail = _currentUserService.GetEmail();
            if (string.IsNullOrEmpty(userEmail))
                return BadRequest(new { success = false, message = _localizationHelper.UserEmailNotProvided });

            var currentUser = await _userProfileService.GetByEmailAsync(userEmail);
            if (currentUser == null)
                return BadRequest(new { success = false, message = _localizationHelper.UserNotFound });

            var firstCompanyId = _currentUserService.GetCompany();
            if (firstCompanyId == null)
                return BadRequest(new { success = false, message = _localizationHelper.UserCompanyNotFound });

            if (!Guid.TryParse(firstCompanyId, out Guid firstCompany))
                return BadRequest(new { success = false, message = _localizationHelper.InvalidCompanyId });

            var existingInviteFromThisCompany = await _businessInviteService
                .GetBusinessInviteByEmailAndCompany(dto.Email, firstCompany);

            if (existingInviteFromThisCompany != null)
                return BadRequest(new { success = false, message = _localizationHelper.InviteAlreadySentToThisUser });

            var targetUser = await _userProfileService.GetByEmailAsync(dto.Email);
            if (targetUser != null)
            {
                if (targetUser.Role == UserRole.Admin)
                    return BadRequest(new { success = false, message = _localizationHelper.CannotInviteAdmin });

                var isOwnerAnywhere = await _userProfileService.IsUserOwnerInAnyBusiness(targetUser.Id);
                if (isOwnerAnywhere)
                    return BadRequest(new { success = false, message = _localizationHelper.CannotInviteOwner });

                var isAlreadyInBusiness = await _userProfileService.IsUserInBusiness(targetUser.Id, firstCompany);
                if (isAlreadyInBusiness)
                    return BadRequest(new { success = false, message = _localizationHelper.UserAlreadyAssistantInYourBusiness });
            }

            var businessInvite = await _businessInviteService.SendInvite(dto, currentUser.Id, firstCompany);
            if (businessInvite == null)
                return BadRequest(new { success = false, message = _localizationHelper.InviteTokenCreateFail });

            var registerLink = $"{_urlSettings.Value.FrontendUrl}/api/auth/register";
            var emailBody = _emailTemplateService.GetInviteEmailTemplate(businessInvite.Token, registerLink);

            await _emailService.SendEmailAsync(dto.Email, _localizationHelper.InviteTokenPageTitle, emailBody);

            return Ok(new
            {
                success = true,
                message = _localizationHelper.InviteSent,
                invitedUserExists = targetUser != null
            });
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> RemoveInvite (Guid id)
        {
            try
            {
                await _businessInviteService.RemoveInvite(id);
                return Ok(new { success = true, message = _localizationHelper.InviteRemoved });
            }
            catch (Exception ex)
            {
                return BadRequest(new {success = true, message = ex.Message});
            }
            
        }
    }
}
