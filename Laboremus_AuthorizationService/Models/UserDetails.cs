using Newtonsoft.Json.Linq;

namespace Laboremus_AuthorizationService.Models
{
    public class UserDetails
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public JObject Claims { get; set; }

        public UserDetails()
        {
            Claims = new JObject();
        }
    }
}