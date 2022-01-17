using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Repositories;
using Laboremus_AuthorizationService.Repositories.Client;
using ClientModel = IdentityServer4.EntityFramework.Entities.Client;

namespace Laboremus_AuthorizationService.Services.Clients
{
    /// <summary>
    /// Client Service
    /// </summary>
    public class ClientService : ServiceBase<ClientModel, ClientViewModel>, IClientService
    {
        private readonly IClientRepository _repository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Client Service Constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="mapper"></param>
        public ClientService(IClientRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
    }
}