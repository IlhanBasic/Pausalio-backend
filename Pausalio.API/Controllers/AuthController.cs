using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pausalio.Application.DTOs.Authentication;
using Pausalio.Application.DTOs.BusinessInvite;
using Pausalio.Application.DTOs.UserProfile;
using Pausalio.Application.Helpers;
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

        /// <summary>
        /// Služi za autentifikaciju korisnika putem Google naloga. Prima GoogleLoginRequestDto koji sadrži token dobijen od Google-a, validira ga i ako je validan, kreira ili pronalazi korisnika u bazi, te vraća JWT token za autentifikaciju u aplikaciji.
        /// </summary>
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
        {
            try
            {
                var user = await _userProfileService.GoogleAuthentication(request);
                var token = _jwtService.GenerateToken(user);
                return Ok(new { success = true, message = _localizationHelper.LoginSuccessfull, token = token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = "Unauthorized", message = ex.Message });
            }
        }

        /// <summary>
        /// Služi za osvežavanje JWT tokena. Korisnik koji je već autentifikovan može pozvati ovu rutu da dobije novi token sa produženim vremenom važenja. Ruta je zaštićena Authorize atributom, što znači da korisnik mora biti autentifikovan da bi mogao da je koristi. U metodi se uzima email trenutnog korisnika iz konteksta, pronalazi se korisnik u bazi i generiše se novi token koji se vraća u odgovoru.
        /// </summary>
        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            var userEmail = _currentUserService.GetEmail();
            if (userEmail == null)
                return Unauthorized(new { success = false, message = _localizationHelper.Unauthorized });
            var user = await _userProfileService.GetByEmailAsync(userEmail);
            if (user == null)
                return NotFound(new { success = false, message = _localizationHelper.UserNotFound });
            var newToken = _jwtService.GenerateToken(user);
            return Ok(new { token = newToken });
        }

        /// <summary>
        /// Služi za autentifikaciju korisnika putem emaila i lozinke. Prima LoginDto koji sadrži email i lozinku, validira ih i ako su ispravni, pronalazi korisnika u bazi, generiše JWT token i postavlja ga u HttpOnly cookie. Ako su kredencijali neispravni, vraća Unauthorized odgovor.
        /// </summary>
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

        /// <summary>
        /// Služi za prihvatanje pozivnice za pridruživanje poslovnom profilu. Korisnik koji je pozvan da se pridruži poslovnom profilu kao asistent može koristiti ovu rutu da prihvati pozivnicu. Ruta je zaštićena Authorize atributom, što znači da korisnik mora biti autentifikovan da bi mogao da je koristi. U metodi se uzima email trenutnog korisnika iz konteksta, pronalazi se korisnik u bazi, zatim se proverava validnost pozivnice na osnovu emaila i tokena. Ako je sve validno, korisnik se dodaje kao asistent u poslovni profil, a pozivnica se briše iz baze. Transakcija se koristi kako bi se osiguralo da su sve operacije atomarne.
        /// </summary>
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
                    throw new Exception(_localizationHelper.BusinesProfileNotFound);
                }

                var userBusiness = await _userProfileService.AddUserToBusinessProfile(
                    existingUser.Id,
                    business.Id,
                    UserBusinessRole.Assistant
                );

                if (userBusiness == null)
                {
                    throw new Exception(_localizationHelper.FailedToAddUserToBusiness);
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

        /// <summary>
        /// Služi za registraciju novog admin korisnika. Prima AddUserProfileDto koji sadrži informacije o korisniku, validira da li već postoji korisnik sa istim emailom i ako ne postoji, kreira novog korisnika sa ulogom Admin. Ruta je zaštićena Authorize atributom sa rolom Admin, što znači da samo korisnici sa ulogom Admin mogu koristiti ovu rutu da kreiraju nove admin naloge.
        /// </summary>
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

        /// <summary>
        /// Služi za registraciju novog asistenta na osnovu pozivnice. Prima RegisterAssistantDto koji sadrži informacije o korisniku i token pozivnice, validira da li već postoji korisnik sa istim emailom, proverava validnost pozivnice i ako je sve validno, kreira novog korisnika sa ulogom RegularUser, dodaje ga kao asistenta u poslovni profil i briše pozivnicu iz baze. Nakon uspešne registracije, šalje se email sa linkom za verifikaciju email adrese.
        /// </summary>
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
                if (newUser == null)
                    throw new Exception(_localizationHelper.RegistrationFailed);
                var business = await _userProfileService.GetCompanyById(invite.BusinessProfileId);
                if (business == null)
                    throw new Exception (_localizationHelper.BusinesProfileNotFound);

                var userBusiness = await _userProfileService.AddUserToBusinessProfile(newUser!.Id, business.Id, UserBusinessRole.Assistant);

                if (userBusiness == null)
                    throw new Exception(_localizationHelper.RegistrationFailed);
                await _userProfileService.DeleteBusinessInvite(invite.Id);
                var verificationToken = Guid.NewGuid().ToString();
                await _userProfileService.SetEmailVerificationToken(
                    newUser.Id,
                    verificationToken,
                    DateTime.UtcNow.AddHours(24)
                );
                await transaction.CommitAsync();
                
                var verificationLink = $"{_urlSettings.Value.FrontendUrl}/verify-email?token={verificationToken}&email={newUser.Email}";
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

        /// <summary>
        /// Služi za registraciju novog vlasnika poslovnog profila. Prima RegisterOwnerDto koji sadrži informacije o korisniku i poslovnom profilu, validira da li već postoji korisnik sa istim emailom i da li već postoji poslovni profil sa istim PIB-om ili MB-om. Ako su validacije uspešne, kreira novog korisnika sa ulogom RegularUser, kreira novi poslovni profil, dodaje korisnika kao vlasnika u poslovni profil i šalje email sa linkom za verifik
        /// </summary>
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
                    throw new Exception(_localizationHelper.RegistrationFailed);
                }

                var business = await _userProfileService.CreateBusinessProfileOnly(dto.Business);
                if (business == null)
                {
                    throw new Exception(_localizationHelper.RegistrationFailed);
                }

                var userBusinessProfile = await _userProfileService.AddUserToBusinessProfile(newUser.Id, business.Id, UserBusinessRole.Owner);

                if (userBusinessProfile == null)
                {
                    throw new Exception(_localizationHelper.RegistrationFailed);
                }

                var verificationToken = Guid.NewGuid().ToString();
                await _userProfileService.SetEmailVerificationToken(
                    newUser.Id,
                    verificationToken,
                    DateTime.UtcNow.AddHours(24)
                );

                await transaction.CommitAsync();

                var verificationLink = $"{_urlSettings.Value.FrontendUrl}/verify-email?token={verificationToken}&email={newUser.Email}";
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

        /// <summary>
        /// Služi za verifikaciju email adrese korisnika. Prima token i email kao query parametre, validira da li su token i email validni, proverava da li token odgovara onome koji je sačuvan u bazi za datu email adresu i da li nije istekao. Ako je sve validno, označava email adresu kao verifikovanu u bazi. Ova ruta se obično poziva kada korisnik klikne na link za verifikaciju koji je poslat na njegovu email adresu nakon registracije.
        /// </summary>
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token, [FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
                return BadRequest(new { success = false, message = _localizationHelper.InvalidRequest });

            var userProfile = await _userProfileService.GetByEmailAsync(email);
            if (userProfile == null)
                return NotFound(new { success = false, message = _localizationHelper.UserNotFound });

            var isValid = await _userProfileService.VerifyEmailToken(email, token);
            if (!isValid)
                return BadRequest(new { success = false, message = _localizationHelper.InvalidOrExpiredToken });

            await _userProfileService.MarkEmailAsVerified(userProfile.Id);

            return Ok(new { success = true });
        }

        /// <summary>
        /// Služi za ponovno slanje emaila za verifikaciju. Prima ResendVerificationDto koji sadrži email adresu korisnika, validira da li postoji korisnik sa datom email adresom i da li već nije verifikovan. Ako su validacije uspešne, generiše novi token za verifikaciju, čuva ga u bazi i šalje novi email sa linkom za verifikaciju na korisnikovu email adresu.
        /// </summary>
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerificationEmail(ResendVerificationDto resendVerificationDto)
        {
            var userProfile = await _userProfileService.GetByEmailAsync(resendVerificationDto.Email);
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

            var verificationLink = $"{_urlSettings.Value.FrontendUrl}/verify-email?token={verificationToken}&email={userProfile.Email}";
            var emailBody = _emailTemplateService.GetVerifyEmailTemplate(userProfile.FirstName, verificationLink);
            await _emailService.SendEmailAsync(userProfile.Email, _localizationHelper.ConfirmEmail, emailBody);

            return Ok(new { success = true, message = _localizationHelper.VerificationEmailResent });
        }

        /// <summary>
        /// Služi za odjavu korisnika. Ruta je zaštićena Authorize atributom, što znači da korisnik mora biti autentifikovan da bi mogao da je koristi. U metodi se postavlja cookie
        /// </summary>
        [HttpPost("logout")]
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

        /// <summary>
        /// Služi za promenu lozinke korisnika. Ruta je zaštićena Authorize atributom, što znači da korisnik mora biti autentifikovan da bi mogao da je koristi. Prima ChangePasswordDto koji sadrži staru i novu lozinku, validira da li su polja popunjena, zatim poziva servis koji proverava da li stara lozinka odgovara onoj koja je sačuvana u bazi i ako je validacija uspešna, menja lozinku na novu. Ako dođe do bilo kakve greške tokom procesa, vraća se BadRequest sa odgovarajućom porukom.
        /// </summary>
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

        /// <summary>
        /// Služi za iniciranje procesa zaboravljene lozinke. Prima ForgotPasswordDto koji sadrži email adresu korisnika, validira da li je polje popunjeno, zatim generiše PIN kod za resetovanje lozinke, čuva ga u bazi zajedno sa vremenom isteka i šalje email korisniku sa PIN kodom. Ako dođe do bilo kakve greške tokom procesa, vraća se BadRequest sa odgovarajućom porukom. Ova ruta se obično poziva kada korisnik klikne na link "Zaboravili ste lozinku?" na stranici za prijavu.
        /// </summary>
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

        /// <summary>
        /// Služi za resetovanje lozinke korisnika. Prima ResetPasswordDto koji sadrži email adresu, PIN kod i novu lozinku, validira da li su polja popunjena, zatim poziva servis koji proverava da li PIN kod odgovara onome koji je sačuvan u bazi za datu email adresu i da li nije istekao. Ako je validacija uspešna, menja lozinku na novu. Ako dođe do bilo kakve greške tokom procesa, vraća se BadRequest sa odgovarajućom porukom. Ova ruta se obično poziva kada korisnik unese PIN kod koji je dobio na email nakon iniciranja procesa zaboravljene lozinke.
        /// </summary>
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