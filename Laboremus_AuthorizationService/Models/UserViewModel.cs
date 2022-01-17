using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace Laboremus_AuthorizationService.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Username { get; set; }

        public string Telephone { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string Role { get; set; }
    }

    /// <summary>
    /// Register user
    /// </summary>
    public class RegisterUserViewModel
    {
        /// <summary>
        /// First Name
        /// </summary>
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        [Required]
        [Display(Name = "Username")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Username can only contain letters or numbers")]
        [MinLength(6, ErrorMessage = "Password is too short")]
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Confirm password
        /// </summary>
        [Compare("Password", ErrorMessage = "Passwords don't match")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Telephone: separate with commas
        /// </summary>
        [Required]
        [Display(Name = "Telephones", Description = "Separate with commas")]
        public string Telephone { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        /// <summary>
        /// User role
        /// </summary>
        [Required]
        [Display(Name = "Role")]
        public UserRole Role { get; set; }
    }

    public class UpdateStatusViewModel
    {
        public string UserId { get; set; }
        public bool LockedOut { get; set; }
    }

    public class UserLockOutViewModel
    {
        [Required]
        public bool? lockedOut { get; set; }
    }

    public class UserEmailViewModel
    {
        [Required]
        public string Email { get; set; }
    }

    public enum UserRole
    {
        /// <summary>
        /// Coop Admin
        /// </summary>
        Administrator = 1,

        /// <summary>
        /// Participant super user
        /// </summary>
        participant_super_user = 2,

        /// <summary>
        /// Sys Admin
        /// </summary>
        sys_admin = 3,

        /// <summary>
        /// Billing operator
        /// </summary>
        billing_operator = 4,

        /// <summary>
        /// Auditor
        /// </summary>
        auditor = 5,

        /// <summary>
        /// Nin Verifier
        /// </summary>
        nin_verifier = 6
    }

    public class NewUserViewModel
    {
        /// <summary>
        /// User's full name
        /// </summary>
        [ProtectedPersonalData]
        [Required] public string Fullname { get; set; }

        /// <summary>
        /// User's role
        /// <example>nin_verifier</example>
        /// </summary>
        [ProtectedPersonalData]
        [Required] public IList<string> Roles { get; set; }

        /// <summary>
        /// The username by which the end-user wants to be referred to at the client application
        /// </summary>
        [ProtectedPersonalData]
        [Required] public string PreferredUsername { get; set; }

        /// <summary>
        /// User password
        /// </summary>
        [ProtectedPersonalData]
        public string Password { get; set; }

        /// <summary>
        /// The end-user's preferred telephone number
        /// </summary>
        [ProtectedPersonalData] public string Telephone { get; set; }

        /// <summary>
        /// The end-user's preferred email address
        /// </summary>
        [ProtectedPersonalData]
        [JsonIgnore] public string Email { get; set; }

        public Dictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();
    }
    
    public class EditUserViewModel
    {
        /// <summary>
        /// User's full name
        /// </summary>
        [ProtectedPersonalData]
        [Required] public string Fullname { get; set; }

        /// <summary>
        /// User's role
        /// <example>nin_verifier</example>
        /// </summary>
        [ProtectedPersonalData]
        [Required] public IList<string> Roles { get; set; }

        /// <summary>
        /// The username by which the end-user wants to be referred to at the client application
        /// </summary>
        [ProtectedPersonalData]
        [Required] public string PreferredUsername { get; set; }

        /// <summary>
        /// The end-user's preferred telephone number
        /// </summary>
        [ProtectedPersonalData] public string Telephone { get; set; }

        /// <summary>
        /// The end-user's preferred email address
        /// </summary>
        [ProtectedPersonalData]
        [JsonIgnore] public string Email { get; set; }

        public Dictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();
    }

    public class Address
    {
        /// <summary>
        /// The full mailing address, with multiple lines if necessary. Newlines can be represented either as a \r\n or as a \n
        /// </summary>
        public string Formatted { get; set; }

        /// <summary>
        /// The street address component, which may include house number, street name, post office box, and other multi-line information. Newlines can be represented either as a \r\n or as a \n
        /// </summary>
        public string StreetAddress { get; set; }

        /// <summary>
        /// City or locality component
        /// </summary>
        public string Locality { get; set; }

        /// <summary>
        /// State, province, prefecture or region component
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Zip code or postal code component
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Country name component
        /// </summary>
        public string Country { get; set; }
    }

    public class SearchRequest
    {
        /// <summary>
        /// User Id. This is an exact match
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User email. Uses full text search. Returns all users whose email matches the given criterion
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Name. Uses full text search. Returns all users whose full name, first name or last name matches the given criterion
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Username. Uses full text search. Returns all users whose username matches the given criterion
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Defaults to 1
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Defaults to 50
        /// </summary>
        public int ItemsPerPage { get; set; } = 50;
    }
}
