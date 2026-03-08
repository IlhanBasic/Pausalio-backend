using Microsoft.Extensions.Options;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Configuration;
using System.Net;
using System.Net.Mail;

namespace Pausalio.Application.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = CreateSmtpClient();
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.From, "Pausalio"),
                To = { to },
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            await client.SendMailAsync(mailMessage);
        }

        public async Task SendEmailWithAttachmentAsync(
            string to,
            string subject,
            string body,
            byte[] attachmentBytes,
            string attachmentFileName)
        {
            using var client = CreateSmtpClient();
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.From, "Pausalio"),
                To = { to },
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            var stream = new MemoryStream(attachmentBytes);
            var attachment = new Attachment(stream, attachmentFileName, "application/pdf");
            mailMessage.Attachments.Add(attachment);

            await client.SendMailAsync(mailMessage);
        }

        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient(_smtpSettings.SmtpHost, _smtpSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_smtpSettings.SmtpUser, _smtpSettings.SmtpPass),
                EnableSsl = true
            };
        }
    }
}