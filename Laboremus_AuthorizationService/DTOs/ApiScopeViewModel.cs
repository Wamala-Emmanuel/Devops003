using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using Newtonsoft.Json;

namespace Laboremus_AuthorizationService.DTOs
{
    public class ApiScopeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public bool Emphasize { get; set; }
        public bool ShowInDiscoveryDocument { get; set; } = true;
        public List<ApiScopeClaimViewModel> UserClaims { get; set; }

        public int ApiResourceId { get; set; }
            
        [JsonIgnore]
        public ApiResource ApiResource { get; set; }
    }
}
