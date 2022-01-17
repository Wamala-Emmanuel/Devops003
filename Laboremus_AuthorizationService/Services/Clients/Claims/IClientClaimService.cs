using IdentityServer4.EntityFramework.Entities;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.ViewModels;

namespace Laboremus_AuthorizationService.Services.Clients.Claims
{
    /// <summary>
    /// Client Service
    /// </summary>
    public interface IClientClaimService : IServiceBase<ClientClaim, ClientClaimViewModel>
    {
    }
}
