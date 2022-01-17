using System.ComponentModel.DataAnnotations;

namespace Laboremus_AuthorizationService.Models
{
    /// <summary>
    /// Revoke Tokens View Model
    /// </summary>
    public class RevokeTokensViewModel
    {
        /// <summary>
        /// Access Token to revoke
        /// </summary>
        [Required]
        public string AccessToken { get; set; }

        /// <summary>
        /// Refresh token to revoke
        /// </summary>
        [Required]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Client details
        /// </summary>
        [Required]
        public ClientCredentials Client { get; set; }
    }
}