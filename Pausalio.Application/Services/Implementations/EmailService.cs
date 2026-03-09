using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Configuration;

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
            var message = BuildMessage(to, subject, body);
            await SendAsync(message);
        }

        public async Task SendEmailWithAttachmentAsync(
            string to,
            string subject,
            string body,
            byte[] attachmentBytes,
            string attachmentFileName)
        {
            var message = BuildMessage(to, subject, body);

            var builder = new BodyBuilder { HtmlBody = body };
            builder.Attachments.Add(attachmentFileName, attachmentBytes,
                new MimeKit.ContentType("application", "pdf"));
            message.Body = builder.ToMessageBody();

            await SendAsync(message);
        }

        private MimeMessage BuildMessage(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Pausalio", _smtpSettings.From));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };
            return message;
        }

        private async Task SendAsync(MimeMessage message)
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpSettings.SmtpHost, _smtpSettings.SmtpPort,
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpSettings.SmtpUser, _smtpSettings.SmtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}