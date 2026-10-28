using Pausalio.Application.DTOs.Invoice;
using Pausalio.Shared.Enums;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IEmailTemplateService
    {
        string GetVerifyEmailTemplate(string firstName, string verificationLink);
        string GetInviteEmailTemplate(string token, string registerLink, bool userExists);
        string GetPasswordResetPinTemplate(string firstName, string pin);
        string GetUnreadMessageNotificationTemplate(string senderName, string appUrl);
        string GetInvoiceEmailTemplate(InvoiceExportDto data, Language lang);
    }
}
