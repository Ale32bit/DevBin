using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DevBin.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly SMTPConfig _smtpConfig;

        public EmailSender(ILogger<EmailSender> logger, IOptions<SMTPConfig> options)
        {
            _logger = logger;
            _smtpConfig = options.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_smtpConfig.Name, _smtpConfig.MailAddress));
                message.To.Add(new MailboxAddress(email, email));
                message.Subject = subject;

                message.Body = new BodyBuilder
                {
                    HtmlBody = htmlMessage
                }.ToMessageBody();

                var client = new SmtpClient();
                await client.ConnectAsync(_smtpConfig.Host, _smtpConfig.Port);
                await client.AuthenticateAsync(_smtpConfig.Username, _smtpConfig.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
    }
}
