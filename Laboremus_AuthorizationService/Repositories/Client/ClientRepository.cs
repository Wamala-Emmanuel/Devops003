using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Interfaces;

namespace Laboremus_AuthorizationService.Repositories.Client
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientRepository : GenericRepository<IdentityServer4.EntityFramework.Entities.Client>, IClientRepository
    {
        private readonly IConfigurationDbContext _context;

        /// <inheritdoc />
        public ClientRepository(ConfigurationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}