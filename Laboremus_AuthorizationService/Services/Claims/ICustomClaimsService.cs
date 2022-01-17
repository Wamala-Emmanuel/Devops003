using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Laboremus_AuthorizationService.Models;

namespace Laboremus_AuthorizationService.Services.Claims
{
    public interface ICustomClaimsService
    {
        Task<List<Claim>> GetExtraClaimsAsync(ApplicationUser user);

        Task<bool> AddClaims(List<CustomClaim> customClaims);

        Task<List<CustomClaim>> GetClaims(string id);

        Task<CustomClaim> EditClaims(CustomClaim customClaim);

        Task DeleteClaims(string id);
    }
}