namespace Laboremus_AuthorizationService.Models
{
    public class AzureAdB2CProvider
    {
        public string Domain { get; set; }
        public string Instance { get; set; }
        public string ClientId { get; set; }
        public string CallbackPath { get; set; }
        public string SignUpSignInPolicyId { get; set; }
        public string ResetPasswordPolicyId { get; set; }
        public string EditProfilePolicyId { get; set; }
    }

    public class ExternalProvider
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public Settings Settings { get; set; }
    }

    public class Settings
    {
        public string Instance { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }    
        public string ResponseType { get; set; }    
        public string CallbackPath { get; set; }    
        public string PostLogoutCallbackPath { get; set; }
        public string Domain { get; set; }
        public string Scopes { get; set; }
        public string SignUpSignInPolicyId { get; set; }
        public string ResetPasswordPolicyId { get; set; }
        public string EditProfilePolicyId { get; set; }

    }
}
