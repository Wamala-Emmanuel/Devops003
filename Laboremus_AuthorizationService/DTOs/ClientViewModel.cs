using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Models;
using Client = IdentityServer4.EntityFramework.Entities.Client;

namespace Laboremus_AuthorizationService.DTOs
{
    public class ClientViewModel : Client
    {   
        public List<ClientSecretViewModel> ClientSecrets { get; set; }
        public List<ClientGrantTypeViewModel> AllowedGrantTypes { get; set; }
        public List<ClientRedirectUriViewModel> RedirectUris { get; set; }
        public List<ClientPostLogoutRedirectUriViewModel> PostLogoutRedirectUris { get; set; }
        public List<ClientScopeViewModel> AllowedScopes { get; set; }
        public List<ClientIdPRestrictionViewModel> IdentityProviderRestrictions { get; set; }
        public List<ClientClaimViewModel> Claims { get; set; }
        public List<ClientCorsOriginViewModel> AllowedCorsOrigins { get; set; }
        public List<ClientPropertyViewModel> Properties { get; set; }
    }
}
