using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using Newtonsoft.Json;

namespace Laboremus_AuthorizationService.DTOs
{
    public class IdentityResourceViewModel : IdentityResource
    {
        //[JsonIgnore] public List<UserClaim> UserClaims { get; set; }
        //[JsonIgnore] public List<IdentityResourceProperty> Properties { get; set; }
    }
}
