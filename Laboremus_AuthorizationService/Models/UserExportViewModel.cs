using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace Laboremus_AuthorizationService.Models
{
    public class UserExportViewModel
    {
        /// <summary>
        /// The user's identifier
        /// </summary>
        public string Id { get; set; }



        /// <summary>
        /// User's full name
        /// </summary>
        [ProtectedPersonalData]
        public string Fullname { get; set; }


        /// <summary>
        /// The end-user's preferred email address
        /// </summary>
        [ProtectedPersonalData]
        [JsonIgnore] public string Email { get; set; }

        /// <summary>
        /// User's role
        /// <example>nin_verifier</example>
        /// </summary>
        [ProtectedPersonalData]
        public string Role { get; set; }

        /// <summary>
        /// Shows whether the user is active or not in the system
        /// </summary>
        public bool Status { get; set; }

        [JsonIgnore] public List<Claim> Claims { get; set; } = new List<Claim>();
    }
}
