using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pausalio.Application.DTOs.UserProfile;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Configuration;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IJwtService _jwtService;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IOptions<UrlSettings> _urlSettings;

        public AuthController(
            IUserProfileService userProfileService,
            IJwtService jwtService,
            ILocalizationHelper localizationHelper,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService,
            IOptions<UrlSettings> urlSettings)
        {
            _userProfileService = userProfileService;
            _jwtService = jwtService;
            _localizationHelper = localizationHelper;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _urlSettings = urlSettings;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var user = await _userProfileService.LoginAsync(dto.Email, dto.Password);
               
                var token = _jwtService.GenerateToken(user!);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(60)
                };

                Response.Cookies.Append("access_token", token, cookieOptions);

                return Ok(new { message = _localizationHelper.LoginSuccessfull, Token = token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new {message = ex.Message});
            }
        }


        [HttpPost("register-assistant")]
        public async Task<IActionResult> RegisterAssistant([FromBody] RegisterAssistantDto dto)
        {
            var existingUser = await _userProfileService.GetByEmailAsync(dto.User.Email);
            if (existingUser != null)
                return BadRequest(new { message = _localizationHelper.UserAlreadyExists});
            var invite = await _userProfileService.GetBusinessInvite(dto.User.Email, dto.InviteToken);
            if (invite == null)
                return BadRequest(new { message = _localizationHelper.InviteTokenDismatch});
            await using var transaction = await _userProfileService.BeginTransactionAsync();
            try
            {
                var newUser = await _userProfileService.CreateUserProfile(dto.User);
                
                var business = await _userProfileService.GetCompanyById(invite.BusinessProfileId);
                if (business == null)
                    return BadRequest(new {message = _localizationHelper.BusinesProfileNotFound});

                var userBusiness = await _userProfileService.AddUserToBusinessProfile(newUser!.Id, business.Id, Shared.Enums.UserRole.Assistant);

                if (userBusiness == null)
                    return BadRequest(new { message = _localizationHelper.RegistrationFailed});
                await transaction.CommitAsync();
                
                var verificationToken = Guid.NewGuid().ToString();
                await _userProfileService.SetEmailVerificationToken(
                    newUser.Id,
                    verificationToken,
                    DateTime.UtcNow.AddHours(24)
                );

                var verificationLink = $"{_urlSettings.Value.BackendUrl}/api/auth/verify-email?token={verificationToken}&email={newUser.Email}";
                var emailBody = _emailTemplateService.GetVerifyEmailTemplate(newUser.FirstName, verificationLink);

                await _emailService.SendEmailAsync(newUser.Email, _localizationHelper.ConfirmEmail, emailBody);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new {message =  ex.Message});
            }
            return Ok();
        }

        [HttpPost("register-owner")]
        public async Task<IActionResult> RegisterOwner([FromBody] RegisterOwnerDto dto)
        {
            var existingUser = await _userProfileService.GetByEmailAsync(dto.User.Email);
            if (existingUser != null)
                return BadRequest(new { message = _localizationHelper.UserAlreadyExists});
            var existingBusiness = await _userProfileService.GetCompanyByPibOrMb(dto.Business.PIB, dto.Business.MB);
            if (existingBusiness != null)
                return BadRequest(new { message = _localizationHelper.CompanyAlreadyExists});

            await using var transaction = await _userProfileService.BeginTransactionAsync();

            try
            {
                var newUser = await _userProfileService.CreateUserProfile(dto.User);
                if (newUser == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = _localizationHelper.RegistrationFailed});
                }

                var business = await _userProfileService.CreateBusinessProfileOnly(dto.Business);
                if (business == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = _localizationHelper.RegistrationFailed});
                }

                var userBusinessProfile = await _userProfileService.AddUserToBusinessProfile(newUser.Id, business.Id, Shared.Enums.UserRole.Owner);

                if (userBusinessProfile == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message =_localizationHelper.RegistrationFailed});
                }

                var verificationToken = Guid.NewGuid().ToString();
                await _userProfileService.SetEmailVerificationToken(
                    newUser.Id,
                    verificationToken,
                    DateTime.UtcNow.AddHours(24)
                );

                await transaction.CommitAsync();

                var verificationLink = $"{_urlSettings.Value.BackendUrl}/api/auth/verify-email?token={verificationToken}&email={newUser.Email}";
                var emailBody = _emailTemplateService.GetVerifyEmailTemplate(newUser.FirstName, verificationLink);
                await _emailService.SendEmailAsync(newUser.Email, _localizationHelper.ConfirmEmail, emailBody);

                return Ok();
            }
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();
                return BadRequest(new {message = ex.Message});
            }
        }

        [HttpGet("verify-email")]
        public async Task<ContentResult> VerifyEmail([FromQuery] string token, [FromQuery] string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                var errorHtml = _emailTemplateService.GetVerificationErrorPage(
                    $"{_urlSettings.Value.BackendUrl}/api/auth/resend-verification?email={email}",
                    _urlSettings.Value.BackendUrl,
                    _localizationHelper.InvalidRequest
                );
                return new ContentResult
                {
                    ContentType = "text/html",
                    StatusCode = 400,
                    Content = errorHtml
                };
            }

            var userProfile = await _userProfileService.GetByEmailAsync(email);
            if (userProfile == null)
            {
                var errorHtml = _emailTemplateService.GetVerificationErrorPage(
                    $"{_urlSettings.Value.BackendUrl}/api/auth/resend-verification?email={email}",
                    _urlSettings.Value.BackendUrl,
                    _localizationHelper.UserNotFound
                );
                return new ContentResult
                {
                    ContentType = "text/html",
                    StatusCode = 404,
                    Content = errorHtml
                };
            }

            var isValid = await _userProfileService.VerifyEmailToken(email, token);
            if (!isValid)
            {
                var errorHtml = _emailTemplateService.GetVerificationErrorPage(
                    $"{_urlSettings.Value.BackendUrl}/api/auth/resend-verification?email={email}",
                    _urlSettings.Value.BackendUrl,
                    _localizationHelper.InvalidOrExpiredToken
                );
                return new ContentResult
                {
                    ContentType = "text/html",
                    StatusCode = 400,
                    Content = errorHtml
                };
            }

            await _userProfileService.MarkEmailAsVerified(userProfile.Id);

            var successHtml = _emailTemplateService.GetVerificationSuccessPage(
                $"{_urlSettings.Value.FrontendUrl}/api/auth/login"
            );

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = 200,
                Content = successHtml
            };
        }

        [HttpGet("resend-verification")]
        public async Task<IActionResult> ResendVerificationEmail([FromQuery] string email)
        {
            var userProfile = await _userProfileService.GetByEmailAsync(email);
            if (userProfile == null)
                return BadRequest(new { message = _localizationHelper.UserNotFound});

            if (userProfile.IsEmailVerified)
                return BadRequest(new { message = _localizationHelper.EmailAlreadyVerified});

            var verificationToken = Guid.NewGuid().ToString();
            await _userProfileService.SetEmailVerificationToken(
                userProfile.Id,
                verificationToken,
                DateTime.UtcNow.AddHours(24)
            );

            var verificationLink = $"{_urlSettings.Value.BackendUrl}/api/auth/verify-email?token={verificationToken}&email={userProfile.Email}";
            var emailBody = _emailTemplateService.GetVerifyEmailTemplate(userProfile.FirstName, verificationLink);
            await _emailService.SendEmailAsync(userProfile.Email, _localizationHelper.ConfirmEmail, emailBody);

            return Ok(new { message = _localizationHelper.VerificationEmailResent });
        }
    }
}