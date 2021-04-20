using Application.Interfaces;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Infrastructure.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IOptions<SendGridSettings> Settings;

        public EmailSender(IOptions<SendGridSettings> settings)
        {
            Settings = settings;
        }

        public async Task SendEmailAsync(string userEmail, string emailSubject, string message)
        {
            var client = new SendGridClient(Settings.Value.Key);
            var msg = new SendGridMessage
            {
                From = new EmailAddress("yagoli8686@vreagles.com", Settings.Value.User),
                Subject = emailSubject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(userEmail));
            msg.SetClickTracking(false, false);

            await client.SendEmailAsync(msg);
        }
    }
}