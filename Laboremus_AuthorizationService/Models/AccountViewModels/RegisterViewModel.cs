using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Laboremus_AuthorizationService.Models.AccountViewModels
{
    /// <summary>
    /// Register ViewModel
    /// </summary>
    public class RegisterViewModel
    {
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        
        [Required]
        public string Username { get; set; }

        [Required]
        public string Name { get; set; }

        public List<string> Roles { get; set; } 

    }
}
