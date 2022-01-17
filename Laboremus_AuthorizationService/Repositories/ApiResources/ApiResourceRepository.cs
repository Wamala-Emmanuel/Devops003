using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;

namespace Laboremus_AuthorizationService.Repositories.ApiResources
{
    public class ApiResourceRepository : GenericRepository<ApiResource>, IApiResourceRepository
    {
        private readonly IConfigurationDbContext _context;
        public ApiResourceRepository(ConfigurationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}