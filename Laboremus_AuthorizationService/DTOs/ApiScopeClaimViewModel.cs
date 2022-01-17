using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using Newtonsoft.Json;

namespace Laboremus_AuthorizationService.DTOs
{
    public class ApiScopeClaimViewModel : UserClaim
    {
        public int ApiScopeId { get; set; }

        [JsonIgnore]
        public ApiScope ApiScope { get; set; }
    }
}
