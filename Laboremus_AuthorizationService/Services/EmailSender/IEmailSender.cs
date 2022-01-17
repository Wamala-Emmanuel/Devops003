using System.Threading.Tasks;

namespace Laboremus_AuthorizationService.Services.EmailSender
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
