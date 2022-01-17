using System.ComponentModel.DataAnnotations;

namespace Laboremus_AuthorizationService.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
