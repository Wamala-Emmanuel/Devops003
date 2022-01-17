using Laboremus_AuthorizationService.Core.Exceptions;
using Laboremus_AuthorizationService.Models;
using Laboremus_AuthorizationService.Repositories.Claims;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.ViewModels;
using Newtonsoft.Json;

namespace Laboremus_AuthorizationService.Services.Claims
{
    public class CustomClaimsService : ICustomClaimsService
    {
        private readonly ICustomClaimsRepository _customClaimsRepository;

        public CustomClaimsService(ICustomClaimsRepository customClaimsRepository)
        {
            _customClaimsRepository = customClaimsRepository;
        }

        public async Task<List<Claim>> GetExtraClaimsAsync(ApplicationUser user)
        {
            var custom = await _customClaimsRepository
                .FindBy(claim => claim.Id == user.Id || claim.Id == user.Email)
                .FirstOrDefaultAsync();
            var data = custom?.ClaimsData??"[]";
            return JsonConvert.DeserializeObject<List<ClaimViewModel>>(data).Select(it=>new Claim(it.Type,it.Value)).ToList();
        }

        public async Task<bool> AddClaims(List<CustomClaim> customClaims)
        {
            await _customClaimsRepository.AddRangeAsync(customClaims);
            await _customClaimsRepository.SaveChangesAsync();
            return true;
        }

        public async Task<CustomClaim> EditClaims(CustomClaim customClaim)
        {
            var claim = await _customClaimsRepository.FindBy(it => it.Id == customClaim.Id).FirstOrDefaultAsync();
            if (claim == null)
            {
                throw new ClientFriendlyException($"Invalid data");
            }
            await _customClaimsRepository.UpdateAsync(customClaim.Id, customClaim);
            await _customClaimsRepository.SaveChangesAsync();
            return claim;
        }

        public async Task<List<CustomClaim>> GetClaims(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return await _customClaimsRepository.GetAll().ToListAsync();
            }
            return await _customClaimsRepository.FindBy(it=>it.Id == id).ToListAsync();
        }

        public async Task DeleteClaims(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ClientFriendlyException($"Invalid data: {id}");
            }

            var data = await _customClaimsRepository.FirstOrDefaultAsync(it => it.Id == id);
            if (data ==null)
            {
                throw new ClientFriendlyException($"Invalid data: {id}");
            }
            _customClaimsRepository.Delete(data);
            await _customClaimsRepository.SaveChangesAsync();
        }

    }
}
