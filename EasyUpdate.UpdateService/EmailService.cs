using EasyUpdate.Data.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EasyUpdate.UpdateService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendUpdateFailureEmail(ScheduledUpdate update)
        {
            var toAddress = update.ScheduledByUser?.Email;
            if (string.IsNullOrWhiteSpace(toAddress))
            {
                _logger.LogWarning("Update {Id} has no schedulingUser email; failure email not sent", update.Id);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_config["Smtp:From"]));
            message.To.Add(MailboxAddress.Parse(toAddress));
            message.Subject = $"[EasyUpdate] {update.Status}: {update.SoftwareApp.Name} v{update.TargetVersion}";

            message.Body = new TextPart("plain")
            {
                Text = $@"Hello {update.ScheduledByUser?.FullName},

The scheduled update for '{update.SoftwareApp.Name}' did not complete successfully.

Application: {update.SoftwareApp.Name}
Target version: {update.TargetVersion}
Scheduled for: {update.ScheduledStart:g}
Executed at: {update.ExecutedAt:g}
Finished at: {update.CompletedAt:g}
Status: {update.Status}
Reason: {update.FailureReason}

If the status is 'RolledBack', the application has been restored to the version it had before this update ({update.SoftwareApp.CurrentVersion}). No action is required unless you want to investigate why the deploy script failed.

If the status is 'Failed', the rollback itself could not complete either. Please check the server as soon as possible; the log below points to where the backup was left, if any.

Please review and retry the update on the next work day.

--- Log ---
{update.LogOutput}"
            };

            using var client = new SmtpClient();

            try
            {
                var port = int.Parse(_config["Smtp:Port"] ?? "587");
                await client.ConnectAsync(_config["Smtp:Host"], port, SecureSocketOptions.StartTls);

                var user = _config["Smtp:User"];
                if (!string.IsNullOrWhiteSpace(user))
                    await client.AuthenticateAsync(user, _config["Smtp:Password"]);

                await client.SendAsync(message);
            }
            finally
            {
                if (client.IsConnected)
                    await client.DisconnectAsync(true);
            }
        }
    }
}