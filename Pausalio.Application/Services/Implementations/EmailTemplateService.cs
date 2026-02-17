using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Localization;
using System.Reflection;

namespace Pausalio.Application.Services.Implementations
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly ILocalizationHelper _localizationHelper;

        public EmailTemplateService(ILocalizationHelper localizationHelper)
        {
            _localizationHelper = localizationHelper;
        }

        public string GetVerifyEmailTemplate(string firstName, string verificationLink)
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(assemblyFolder!, "Templates", "VerifyEmail.html");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Email template not found at path: {path}");

            var template = File.ReadAllText(path);

            template = template.Replace("{{VerifyEmail}}", _localizationHelper.EmailVerify)
                               .Replace("{{Greeting}}", string.Format(_localizationHelper.EmailVerifyGreeting, firstName))
                               .Replace("{{Text}}", _localizationHelper.EmailVerifyText)
                               .Replace("{{Button}}", _localizationHelper.EmailVerifyButton)
                               .Replace("{{Fallback}}", _localizationHelper.EmailVerifyFallback)
                               .Replace("{{Footer}}", _localizationHelper.EmailVerifyFooter)
                               .Replace("{{VerificationLink}}", verificationLink);

            return template;
        }

        public string GetVerificationSuccessPage(string loginLink)
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(assemblyFolder!, "Templates", "VerificationSuccess.html");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Verification success template not found at path: {path}");

            var template = File.ReadAllText(path);

            template = template.Replace("{{PageTitle}}", _localizationHelper.VerificationSuccessTitle)
                               .Replace("{{Title}}", _localizationHelper.VerificationSuccessHeading)
                               .Replace("{{Message}}", _localizationHelper.VerificationSuccessMessage)
                               .Replace("{{ButtonText}}", _localizationHelper.GoToLogin)
                               .Replace("{{LoginLink}}", loginLink)
                               .Replace("{{Footer}}", _localizationHelper.EmailVerifyFooter);

            return template;
        }

        public string GetVerificationErrorPage(string resendLink, string homeLink, string errorMessage)
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(assemblyFolder!, "Templates", "VerificationError.html");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Verification error template not found at path: {path}");

            var template = File.ReadAllText(path);

            template = template.Replace("{{PageTitle}}", _localizationHelper.VerificationErrorTitle)
                               .Replace("{{Title}}", _localizationHelper.VerificationErrorHeading)
                               .Replace("{{Message}}", errorMessage)
                               .Replace("{{ResendButtonText}}", _localizationHelper.ResendVerificationEmail)
                               .Replace("{{HomeButtonText}}", _localizationHelper.GoToHome)
                               .Replace("{{ResendLink}}", resendLink)
                               .Replace("{{HomeLink}}", homeLink)
                               .Replace("{{Footer}}", _localizationHelper.EmailVerifyFooter);

            return template;
        }

        public string GetInviteEmailTemplate(string token, string registerLink)
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(assemblyFolder!, "Templates", "InviteTokenEmail.html");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Verification success template not found at path: {path}");

            var template = File.ReadAllText(path);



            template = template.Replace("{{PageTitle}}", _localizationHelper.InviteTokenPageTitle)
                               .Replace("{{Title}}", _localizationHelper.InviteTokenTitle)
                               .Replace("{{Token}}", token)
                               .Replace("{{RegisterLink}}", registerLink)
                               .Replace("{{ButtonText}}", _localizationHelper.Register)
                               .Replace("{{Message}}", _localizationHelper.InviteTokenPageMessage)
                               .Replace("{{Footer}}", _localizationHelper.InviteTokenFooter);

            return template;
        }
        public string GetPasswordResetPinTemplate(string firstName, string pin)
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(assemblyFolder!, "Templates", "PasswordResetPinEmail.html");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Password reset email template not found at path: {path}");

            var template = File.ReadAllText(path);

            template = template.Replace("{{PageTitle}}", _localizationHelper.PasswordReset)
                               .Replace("{{Title}}", string.Format(_localizationHelper.PasswordResetGreeting, firstName))
                               .Replace("{{Message}}", _localizationHelper.PasswordResetText)
                               .Replace("{{Token}}", pin)
                               .Replace("{{Footer}}", _localizationHelper.EmailVerifyFooter);

            return template;
        }
    }
}
