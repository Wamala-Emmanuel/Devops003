using AutoMapper;
using IdentityServer4.EntityFramework.Entities;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Repositories;
using Laboremus_AuthorizationService.Repositories.IdentityResources;

namespace Laboremus_AuthorizationService.Services.IdentityResources
{
    public class IdentityResourceService : ServiceBase<IdentityResource, IdentityResourceViewModel>, IIdentityResourceService
    {
        private readonly IIdentityResourceRepository _repository;
        private readonly IMapper _mapper;

        public IdentityResourceService(IIdentityResourceRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
    }
}