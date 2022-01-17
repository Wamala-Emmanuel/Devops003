using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Laboremus_AuthorizationService.Models.AccountViewModels
{
    /// <summary>
    /// Login ViewModel
    /// </summary>
    public class UserLoginViewModel
    {
        /// <summary>
        /// Username
        /// </summary>
        [Required]
        [EmailAddress]
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Client Id
        /// </summary>
        [Required]
        public string ClientId { get; set; }

        /// <summary>
        /// Client Secret
        /// </summary>
        [Required]
        public string ClientSecret { get; set; }
            
        /// <summary>
        /// The resources the client wants to access
        /// <example>openid, profile, offline_access</example>
        /// </summary>
        public List<string> RequestingAccessTo { get; set; }

        /// <summary>
        /// Identity Provider
        /// </summary>
        public IdentityProvider IdentityProvider { get; set; }

        /// <inheritdoc />
        public UserLoginViewModel()
        {
            IdentityProvider = IdentityProvider.LocalUsers;
        }

    }

    /// <inheritdoc />
    public enum IdentityProvider
    {
        /// <summary>
        /// Local users 
        /// </summary>
        LocalUsers = 1,

        /// <summary>
        /// On Premise Active Directory
        /// </summary>
        OnPremiseActiveDirectory = 2,

        /// <summary>
        /// Azure Active Diretory
        /// </summary>
        AzureAd = 3
    }
}
