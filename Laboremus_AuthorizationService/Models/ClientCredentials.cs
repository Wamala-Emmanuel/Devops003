using System.ComponentModel.DataAnnotations;

namespace Laboremus_AuthorizationService.Models
{
    /// <summary>
    /// Client Details
    /// </summary>
    public class ClientCredentials
    {
        /// <summary>
        /// Client ID
        /// </summary>
        [Required]
        public string ClientId { get; set; }

        /// <summary>
        /// Client Secret
        /// </summary>
        [Required]
        public string ClientSecret { get; set; }
    }
}