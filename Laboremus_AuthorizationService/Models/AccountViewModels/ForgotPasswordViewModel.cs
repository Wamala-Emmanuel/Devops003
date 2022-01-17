using System.ComponentModel.DataAnnotations;

namespace Laboremus_AuthorizationService.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
