using AutoMapper;
using IdentityServer4.EntityFramework.Entities;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Repositories;
using Laboremus_AuthorizationService.ViewModels;

namespace Laboremus_AuthorizationService.Services.Clients.Claims
{
    /// <summary>
    /// Client Service
    /// </summary>
    public class ClientClaimService : ServiceBase<ClientClaim, ClientClaimViewModel>, IClientClaimService
    {
        private readonly IGenericRepository<ClientClaim> _repository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Client Service Constructor
        /// </summary>
        /// <param name="repository"></param>
        public ClientClaimService(IGenericRepository<ClientClaim> repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
    }
}