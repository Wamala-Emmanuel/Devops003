using Laboremus_AuthorizationService.Data;
using Laboremus_AuthorizationService.Models;

namespace Laboremus_AuthorizationService.Repositories.Claims
{

    public interface ICustomClaimsRepository : IGenericRepository<CustomClaim>
    {
    }
    public class CustomClaimsRepository: GenericRepository<CustomClaim>, ICustomClaimsRepository
    {
        public CustomClaimsRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
