using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Reflection;

namespace Pausalio.Functions
{
    public class ReminderNotificationFunction
    {
        private readonly PausalioFunctionsDbContext _context;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public ReminderNotificationFunction(
            PausalioFunctionsDbContext context,
            ILoggerFactory loggerFactory,
            IConfiguration config)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<ReminderNotificationFunction>();
            _config = config;
        }

        [Function("ReminderNotificationFunction")]
        public async Task Run([TimerTrigger("0 0 9 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"Reminder job pokrenut: {DateTime.UtcNow}");

            var today = DateTime.UtcNow.Date;

            var reminders = await _context.Reminders
                .Include(r => r.BusinessProfile)
                    .ThenInclude(bp => bp.UserBusinessProfiles)
                        .ThenInclude(ubp => ubp.User)
                .Where(r =>
                    r.DueDate.Date == today &&
                    !r.IsCompleted &&
                    !r.IsDeleted)
                .ToListAsync();

            if (!reminders.Any())
            {
                _logger.LogInformation("Nema remindera za danas.");
                return;
            }

            _logger.LogInformation($"Pronađeno {reminders.Count} remindera za danas.");

            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var templatePath = Path.Combine(assemblyFolder, "Templates", "ReminderEmail.html");
            var template = await File.ReadAllTextAsync(templatePath, System.Text.Encoding.UTF8);

            var grouped = reminders.GroupBy(r => r.BusinessProfileId);

            foreach (var group in grouped)
            {
                var businessProfile = group.First().BusinessProfile;

                var emails = businessProfile.UserBusinessProfiles
                    .Where(ubp => ubp.User?.Email != null)
                    .Select(ubp => ubp.User!.Email)
                    .Distinct()
                    .ToList();

                if (!emails.Any())
                    continue;

                var reminderItems = string.Join("\n", group.Select(r => $"""
                    <div style="padding:10px 0;border-bottom:1px solid #e2e8f0;">
                        <p style="margin:0;font-size:15px;font-weight:600;color:#1e293b;">🕐 {r.Title}</p>
                        {(string.IsNullOrEmpty(r.Description) ? "" : $"<p style='margin:4px 0 0;font-size:13px;color:#64748b;'>{r.Description}</p>")}
                    </div>
                    """));

                var body = template
                    .Replace("{{Date}}", today.ToString("dd.MM.yyyy"))
                    .Replace("{{ReminderItems}}", reminderItems)
                    .Replace("{{DashboardUrl}}", "https://app-pausalio.netlify.app");

                foreach (var email in emails)
                {
                    try
                    {
                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress("Pausalio", _config["SmtpUser"]!));
                        message.To.Add(MailboxAddress.Parse(email));
                        message.Subject = $"📅 Pausalio — Podsetnici za {today:dd.MM.yyyy}";
                        message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

                        using var client = new SmtpClient();
                        await client.ConnectAsync(_config["SmtpHost"], int.Parse(_config["SmtpPort"]!), SecureSocketOptions.StartTls);
                        await client.AuthenticateAsync(_config["SmtpUser"], _config["SmtpPass"]);
                        await client.SendAsync(message);
                        await client.DisconnectAsync(true);

                        _logger.LogInformation($"Mejl poslat na: {email}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Greška pri slanju mejla na {email}: {ex.Message}");
                    }
                }
            }

            _logger.LogInformation("Reminder job završen.");
        }
    }
}