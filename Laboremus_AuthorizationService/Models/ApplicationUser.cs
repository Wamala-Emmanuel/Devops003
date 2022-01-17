using System;
using Microsoft.AspNetCore.Identity;

namespace Laboremus_AuthorizationService.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public DateTime LastUpdatedDate { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"Username: {UserName}, Email: {Email}, Id:{Id}";
        }
    }
}
