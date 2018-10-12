using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using Nexplorer.Config;

namespace Nexplorer.Web.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(EmailMessage emailMessage)
        {
            try
            {
                var message = new MimeMessage
                {
                    Subject = emailMessage.Subject,
                    Body = new TextPart(TextFormat.Html) {Text = emailMessage.Content}
                };

                message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
                message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

                using (var emailClient = new SmtpClient())
                {
                    await emailClient.ConnectAsync(Settings.EmailConfig.SmtpServer, Settings.EmailConfig.SmtpPort, true);

                    emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                    await emailClient.AuthenticateAsync(Settings.EmailConfig.SmtpUsername,
                        Settings.EmailConfig.SmtpPassword);

                    await emailClient.SendAsync(message);

                    await emailClient.DisconnectAsync(true);
                }
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError($"Email send to {emailMessage.ToAddresses.First()} failed. {ex.Message}");
            }
        }

        public Task SendEmailConfirmationAsync(string email, string username, string link)
        {
            return SendEmailAsync(new EmailMessage(new EmailAddress { Address = email, Name = username })
            {
                Subject = "Confirm your email",
                Content = $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>"
            });
        }
    }
}
