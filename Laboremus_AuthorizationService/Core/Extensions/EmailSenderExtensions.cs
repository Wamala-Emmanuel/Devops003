using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Laboremus_AuthorizationService.Services;
using Laboremus_AuthorizationService.Services.EmailSender;

namespace Laboremus_AuthorizationService.Core.Extensions
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
        }
        
        public static Task SendResetPasswordEmailAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, "Reset Password", $"{HtmlEncoder.Default.Encode(link)}");
        }
    }
}
