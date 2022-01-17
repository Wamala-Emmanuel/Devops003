using System.Net.Mail;
using System.Threading.Tasks;
using Laboremus.Messaging.Email.Email;
using Laboremus_AuthorizationService.Core.Helpers;
using Microsoft.Extensions.Configuration;

namespace Laboremus_AuthorizationService.Services.EmailSender
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly IEmailService _emailService;

        public EmailSender(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return ExecuteAsync(email, subject, message);
        }

        private async Task ExecuteAsync(string email, string subject, string message)
        {
            var body = StringExtensions.GetResetPasswordTemplate();

            body = body.Replace("{resetPasswordLink}", message);

            var mailMessage = new MailMessage()
            {
                Body = body,
                Subject = subject,
                IsBodyHtml = true
            };
            mailMessage.To.Add(new MailAddress(email));

            await _emailService.SendMailAsync(mailMessage);
        }
    }
}
