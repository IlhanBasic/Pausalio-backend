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
