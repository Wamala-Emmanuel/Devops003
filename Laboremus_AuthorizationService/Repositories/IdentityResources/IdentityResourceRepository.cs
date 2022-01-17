using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;

namespace Laboremus_AuthorizationService.Repositories.IdentityResources
{
    public class IdentityResourceRepository : GenericRepository<IdentityResource>, IIdentityResourceRepository
    {
        private readonly IConfigurationDbContext _context;
        public IdentityResourceRepository(ConfigurationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}