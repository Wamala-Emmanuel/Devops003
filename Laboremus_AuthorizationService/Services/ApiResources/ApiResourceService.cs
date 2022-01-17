using AutoMapper;
using IdentityServer4.EntityFramework.Entities;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Repositories;
using Laboremus_AuthorizationService.Repositories.ApiResources;

namespace Laboremus_AuthorizationService.Services.ApiResources
{
    public class ApiResourceService : ServiceBase<ApiResource, ApiResourceViewModel>, IApiResourceService
    {
        private readonly IApiResourceRepository _repository;
        private readonly IMapper _mapper;

        public ApiResourceService(IApiResourceRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
    }
}