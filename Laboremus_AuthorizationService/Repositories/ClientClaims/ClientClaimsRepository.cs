using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;

namespace Laboremus_AuthorizationService.Repositories.ClientClaims
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientClaimsRepository : GenericRepository<ClientClaim>, IClientClaimsRepository
    {
        private readonly IConfigurationDbContext _context;

        /// <inheritdoc />
        public ClientClaimsRepository(ConfigurationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}