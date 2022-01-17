namespace Laboremus_AuthorizationService.Models
{
    /// <summary>
    /// Refresh token view model
    /// </summary>
    public class RefreshTokenViewModel
    {
        /// <summary>
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Client ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Client Secret
        /// </summary>
        public string ClientSecret { get; set; }
    }
}
