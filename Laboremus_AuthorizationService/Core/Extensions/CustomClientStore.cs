using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace Laboremus_AuthorizationService.Core.Extensions
{
    public class CustomClientStore : IClientStore
    {
        public static IEnumerable<Client> AllClients { get; } = new[]
        {
            new Client
            {
                ClientId = "test_client",
                ClientName = "Test Client",
                AccessTokenLifetime = 60 * 60 * 24,
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                RequireClientSecret = false,
                AllowedScopes =
                {
                    "crm.api"
                }
            }
        };

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            return Task.FromResult(AllClients.FirstOrDefault(c => c.ClientId == clientId));
        }
    }
}
