using System.ComponentModel.DataAnnotations;

namespace Laboremus_AuthorizationService.Models.AccountViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "New password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression(
            @"((?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\W]).{8,})", 
            ErrorMessage = "The {0} must be atleast 8 characters long, contain atleast one lower, one uppercase letters, a digit(0-9) and a special character.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}
