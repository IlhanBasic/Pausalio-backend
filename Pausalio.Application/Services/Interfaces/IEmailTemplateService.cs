using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IEmailTemplateService
    {
        string GetVerifyEmailTemplate(string firstName, string verificationLink);
        string GetVerificationSuccessPage(string loginLink);
        string GetVerificationErrorPage(string resendLink, string homeLink, string errorMessage);
        string GetInviteEmailTemplate(string token, string registerLink);
    }
}
