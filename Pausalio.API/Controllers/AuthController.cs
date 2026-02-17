using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pausalio.Application.DTOs.BusinessInvite;
using Pausalio.Application.DTOs.UserProfile;
using Pausalio.Application.Helpers;
using Pausalio.Application.Services.Implementations;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Configuration;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IJwtService _jwtService;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IOptions<UrlSettings> _urlSettings;

        public AuthController(
            IUserProfileService userProfileService,
            ICurrentUserService currentUserService,
            IJwtService jwtService,
            ILocalizationHelper localizationHelper,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService,
            IOptions<UrlSettings> urlSettings)
        {
            _userProfileService = userProfileService;
            _currentUserService = currentUserService;
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
                if (user == null)
                    return Unauthorized(_localizationHelper.Unauthorized);
                var token = _jwtService.GenerateToken(user);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(60)
                };

                Response.Cookies.Append("access_token", token, cookieOptions);

                return Ok(new { success = true, message = _localizationHelper.LoginSuccessfull, token = token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { success = false, message = ex.Message});
            }
        }
        [HttpPost("accept-invite")]
        [Authorize(Roles = "RegularUser")]
        public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteDto dto)
        {
            var userEmail = _currentUserService.GetEmail();
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized(new { success = false, message = _localizationHelper.Unauthorized });

            var existingUser = await _userProfileService.GetByEmailAsync(userEmail);
            if (existingUser == null)
                return NotFound(new { success = false, message = _localizationHelper.UserNotFound });

            var invite = await _userProfileService.GetBusinessInvite(userEmail, dto.InviteToken);
            if (invite == null)
                return BadRequest(new { success = false, message = _localizationHelper.InviteTokenDismatch });

            var alreadyAssistant = await _userProfileService.IsUserInBusiness(existingUser.Id, invite.BusinessProfileId);
            if (alreadyAssistant)
                return BadRequest(new { success = false, message = _localizationHelper.AlreadyAssistantInBusiness });

            await using var transaction = await _userProfileService.BeginTransactionAsync();
            try
            {
                var business = await _userProfileService.GetCompanyById(invite.BusinessProfileId);
                if (business == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { success = false, message = _localizationHelper.BusinesProfileNotFound });
                }

                var userBusiness = await _userProfileService.AddUserToBusinessProfile(
                    existingUser.Id,
                    business.Id,
                    UserBusinessRole.Assistant
                );

                if (userBusiness == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { success = false, message = _localizationHelper.FailedToAddUserToBusiness });
                }

                await _userProfileService.DeleteBusinessInvite(invite.Id);

                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = _localizationHelper.InviteAcceptedSuccessfully,
                    businessName = business.BusinessName,
                    businessId = business.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("register-admin")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] AddUserProfileDto dto)
        {
            var existingUser = await _userProfileService.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { success = false, message = _localizationHelper.UserAlreadyExists });
            var newUser = await _userProfileService.CreateUserProfile(dto, UserRole.Admin);
            return Ok(new { success = true, message = _localizationHelper.RegistrationSuccessful});
        }
        [HttpPost("register-assistant")]
        public async Task<IActionResult> RegisterAssistant([FromBody] RegisterAssistantDto dto)
        {
            var existingUser = await _userProfileService.GetByEmailAsync(dto.User.Email);
            if (existingUser != null)
                return BadRequest(new { success = false, message = _localizationHelper.UserAlreadyExists});
            var invite = await _userProfileService.GetBusinessInvite(dto.User.Email, dto.InviteToken);
            if (invite == null)
                return BadRequest(new { success = false, message = _localizationHelper.InviteTokenDismatch});
            await using var transaction = await _userProfileService.BeginTransactionAsync();
            try
            {
                var newUser = await _userProfileService.CreateUserProfile(dto.User, UserRole.RegularUser);
                
                var business = await _userProfileService.GetCompanyById(invite.BusinessProfileId);
                if (business == null)
                    return BadRequest(new { success = false, message = _localizationHelper.BusinesProfileNotFound});

                var userBusiness = await _userProfileService.AddUserToBusinessProfile(newUser!.Id, business.Id, UserBusinessRole.Assistant);

                if (userBusiness == null)
                    return BadRequest(new { success = false, message = _localizationHelper.RegistrationFailed});
                await _userProfileService.DeleteBusinessInvite(invite.Id);
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
                return BadRequest(new { success = false, message =  ex.Message});
            }
            return Ok(new { success = true, message = _localizationHelper.RegistrationSuccessful});
        }

        [HttpPost("register-owner")]
        public async Task<IActionResult> RegisterOwner([FromBody] RegisterOwnerDto dto)
        {
            var existingUser = await _userProfileService.GetByEmailAsync(dto.User.Email);
            if (existingUser != null)
                return BadRequest(new { success = false, message = _localizationHelper.UserAlreadyExists});
            var existingBusiness = await _userProfileService.GetCompanyByPibOrMb(dto.Business.PIB, dto.Business.MB);
            if (existingBusiness != null)
                return BadRequest(new { success = false, message = _localizationHelper.CompanyAlreadyExists});

            await using var transaction = await _userProfileService.BeginTransactionAsync();

            try
            {
                var newUser = await _userProfileService.CreateUserProfile(dto.User, UserRole.RegularUser);
                if (newUser == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { success = false, message = _localizationHelper.RegistrationFailed});
                }

                var business = await _userProfileService.CreateBusinessProfileOnly(dto.Business);
                if (business == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { success = false, message = _localizationHelper.RegistrationFailed});
                }

                var userBusinessProfile = await _userProfileService.AddUserToBusinessProfile(newUser.Id, business.Id, UserBusinessRole.Owner);

                if (userBusinessProfile == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { success = false, message =_localizationHelper.RegistrationFailed});
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

                return Ok(new { success = true, message = _localizationHelper.RegistrationSuccessful});
            }
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();
                return BadRequest(new { success = false, message = ex.Message});
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
                return BadRequest(new { success = false, message = _localizationHelper.UserNotFound});

            if (userProfile.IsEmailVerified)
                return BadRequest(new { success = false, message = _localizationHelper.EmailAlreadyVerified});

            var verificationToken = Guid.NewGuid().ToString();
            await _userProfileService.SetEmailVerificationToken(
                userProfile.Id,
                verificationToken,
                DateTime.UtcNow.AddHours(24)
            );

            var verificationLink = $"{_urlSettings.Value.BackendUrl}/api/auth/verify-email?token={verificationToken}&email={userProfile.Email}";
            var emailBody = _emailTemplateService.GetVerifyEmailTemplate(userProfile.FirstName, verificationLink);
            await _emailService.SendEmailAsync(userProfile.Email, _localizationHelper.ConfirmEmail, emailBody);

            return Ok(new { success = true, message = _localizationHelper.VerificationEmailResent });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(-1) 
                };

                Response.Cookies.Append("access_token", "", cookieOptions);

                return Ok(new { success = true, message = _localizationHelper.LogoutSuccessful });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = _localizationHelper.PasswordRequired
                    });
                }

                await _userProfileService.ChangePassword(dto);

                return Ok(new
                {
                    success = true,
                    message = _localizationHelper.PasswordChangedSuccessfully
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new
                {
                    success = false,
                    message = _localizationHelper.EmailRequired
                });
            }

            var pin = PasswordResetPinHelper.GeneratePin();

            await _userProfileService.SetPasswordResetTokenAsync(
                dto.Email,
                pin,
                DateTime.UtcNow.AddMinutes(15)
            );

            var user = await _userProfileService.GetByEmailAsync(dto.Email);

            if (user != null)
            {
                var emailBody = _emailTemplateService.GetPasswordResetPinTemplate(user.FirstName, pin);
                await _emailService.SendEmailAsync(user.Email, _localizationHelper.PasswordReset, emailBody);
            }

            return Ok(new
            {
                success = true,
                message = _localizationHelper.PasswordResetEmailSent
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Pin) ||
                string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest(new
                {
                    success = false,
                    message = _localizationHelper.AllFieldsRequired
                });
            }

            try
            {
                await _userProfileService.ResetPasswordAsync(dto);

                return Ok(new
                {
                    success = true,
                    message = _localizationHelper.PasswordChangedSuccessfully
                });
            }
            catch (KeyNotFoundException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = _localizationHelper.PasswordResetFailed
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception)
            {
                return BadRequest(new
                {
                    success = false,
                    message = _localizationHelper.PasswordResetFailed
                });
            }
        }
    }
}