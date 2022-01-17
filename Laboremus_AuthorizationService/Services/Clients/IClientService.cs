using Laboremus_AuthorizationService.DTOs;

namespace Laboremus_AuthorizationService.Services.Clients
{
    /// <summary>
    /// Client Service
    /// </summary>
    public interface IClientService : IServiceBase<IdentityServer4.EntityFramework.Entities.Client, ClientViewModel>
    {
    }
}
