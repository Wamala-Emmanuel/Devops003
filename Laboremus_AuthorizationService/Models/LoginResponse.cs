namespace Laboremus_AuthorizationService.Models
{
    /// <summary>
    /// User login response
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Access Token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Time in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// User claims 
        /// </summary>
        public object Claims { get; set; }
    }

    public class RegisterResponse
    {
        public string UserId { get; set; }  
    }


    public class IdentityToken
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }    
        public string Username { get; set; }

        public string FullName { get; set; }

        public IdentityToken()
        {
            FullName = $"{FirstName} {LastName}";
        }
    }
}
