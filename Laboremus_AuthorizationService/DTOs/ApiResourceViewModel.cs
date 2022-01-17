using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;

namespace Laboremus_AuthorizationService.DTOs
{
    public class ApiResourceViewModel
    {
        public int Id { get; set; }
        public bool Enabled { get; set; } = true;
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<ApiSecretViewModel> Secrets { get; set; }
        public List<ApiScopeViewModel> Scopes { get; set; }
        public List<ApiResourceClaimViewModel> UserClaims { get; set; }
        public List<ApiResourcePropertyViewModel> Properties { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? Updated { get; set; }
        public DateTime? LastAccessed { get; set; }
        public bool NonEditable { get; set; }
    }
}
