using Pausalio.Application.DTOs.Invoice;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;
using System.Globalization;
using System.Net;

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
            var path = Path.Combine(AppContext.BaseDirectory, "Templates", "VerifyEmail.html");

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
        public string GetInviteEmailTemplate(string token, string registerLink, bool userExists)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Templates", "InviteTokenEmail.html");
            if (!File.Exists(path))
                throw new FileNotFoundException($"Verification success template not found at path: {path}");

            var template = File.ReadAllText(path);
            template = template.Replace("{{PageTitle}}", _localizationHelper.InviteTokenPageTitle)
                               .Replace("{{Title}}", _localizationHelper.InviteTokenTitle)
                               .Replace("{{Token}}", token)
                               .Replace("{{RegisterLink}}", registerLink)
                               .Replace("{{ButtonText}}", _localizationHelper.Register)
                               .Replace("{{Message}}", _localizationHelper.InviteTokenPageMessage)
                               .Replace("{{Footer}}", _localizationHelper.InviteTokenFooter)
                               .Replace("{{ButtonHiddenClass}}", userExists ? "hidden" : "");

            return template;
        }
        public string GetPasswordResetPinTemplate(string firstName, string pin)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Templates", "PasswordResetPinEmail.html");

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

        public string GetUnreadMessageNotificationTemplate(string senderName, string appUrl)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Templates", "UnreadChatMessageEmail.html");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Unread message notification template not found at path: {path}");

            var template = File.ReadAllText(path);
            template = template.Replace("{{SenderName}}", senderName)
                               .Replace("{{AppUrl}}", appUrl);

            return template;
        }

        public string GetInvoiceEmailTemplate(InvoiceExportDto data, Language lang)
        {
            var suffix = lang == Language.English ? "en" : "sr";
            var path = Path.Combine(AppContext.BaseDirectory, "Templates", $"InvoiceEmail_{suffix}.html");

            // Fallback to Serbian if English template doesn't exist
            if (!File.Exists(path) && lang == Language.English)
            {
                path = Path.Combine(AppContext.BaseDirectory, "Templates", "InvoiceEmail_sr.html");
            }

            if (!File.Exists(path))
                throw new FileNotFoundException($"Invoice email template not found at path: {path}");

            var template = File.ReadAllText(path);

            var culture = lang == Language.English
                ? CultureInfo.GetCultureInfo("en-US")
                : CultureInfo.GetCultureInfo("sr-Latn");

            var issueDate = lang == Language.English
                ? data.IssueDate.ToString("MMMM d, yyyy", culture)
                : data.IssueDate.ToString("dd.MM.yyyy");

            var dueDate = data.DueDate.HasValue
                ? (lang == Language.English
                    ? data.DueDate.Value.ToString("MMMM d, yyyy", culture)
                    : data.DueDate.Value.ToString("dd.MM.yyyy"))
                : "—";

            template = template
                .Replace("{{InvoiceNumber}}", data.InvoiceNumber)
                .Replace("{{BusinessName}}", data.BusinessName)
                .Replace("{{BusinessPIB}}", data.BusinessPIB)
                .Replace("{{BusinessEmail}}", data.BusinessEmail)
                .Replace("{{BusinessPhone}}", data.BusinessPhone ?? "")
                .Replace("{{IssueDate}}", issueDate)
                .Replace("{{DueDate}}", dueDate)
                .Replace("{{TotalAmount}}", data.TotalAmount.ToString("N2"))
                .Replace("{{CurrencyDisplay}}", data.CurrencyDisplay)
                .Replace("{{BusinessBankName}}", data.BusinessBankName ?? "")
                .Replace("{{BusinessAccountNumber}}", data.BusinessAccountNumber ?? "")
                .Replace("{{BusinessIBAN}}", data.BusinessIBAN ?? "")
                .Replace("{{BusinessSWIFT}}", data.BusinessSWIFT ?? "")
                .Replace("{{ReferenceNumber}}", data.ReferenceNumber ?? "");

            template = HandleConditionals(template, "HasBankAccount", !string.IsNullOrEmpty(data.BusinessBankName));
            template = HandleConditionals(template, "HasIBAN", !string.IsNullOrEmpty(data.BusinessIBAN));
            template = HandleConditionals(template, "HasSWIFT", !string.IsNullOrEmpty(data.BusinessSWIFT));
            template = HandleConditionals(template, "BusinessPhone", !string.IsNullOrEmpty(data.BusinessPhone));
            template = HandleConditionals(template, "ReferenceNumber", !string.IsNullOrEmpty(data.ReferenceNumber));

            return template;
        }

        private string HandleConditionals(string template, string key, bool condition)
        {
            var ifTag = $"{{{{#if {key}}}}}";
            var endTag = $"{{{{/if}}}}";

            while (template.Contains(ifTag))
            {
                var start = template.IndexOf(ifTag);
                var end = template.IndexOf(endTag, start) + endTag.Length;
                var block = template.Substring(start, end - start);
                var inner = block.Replace(ifTag, "").Replace(endTag, "");

                template = template.Replace(block, condition ? inner : "");
            }

            return template;
        }
    }
}
