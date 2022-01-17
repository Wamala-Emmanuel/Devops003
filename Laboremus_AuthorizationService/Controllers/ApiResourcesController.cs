using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer4.EntityFramework.Entities;
using Laboremus_AuthorizationService.Core.Extensions;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Services.ApiResources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Laboremus_AuthorizationService.Controllers
{
    /// <inheritdoc />
    
    [Route("api/apiResources")]
    public class ApiResourcesController : BaseController
    {
        private readonly IApiResourceService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="mapper"></param>
        public ApiResourcesController(IApiResourceService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all API Resources
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<ApiResourceViewModel>> Get()
        {
            var resources = await _service.GetAll()
                .Include(q => q.Scopes)
                .ThenInclude(s => s.UserClaims)
                .Include(q => q.UserClaims)
                .Include(q => q.Properties)
                .Include(q => q.Secrets)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ApiResourceViewModel>>(resources);
        }

        /// <summary>
        /// Get an API Resource by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ApiResourceViewModel> Get(int id)
        {
            return await _service.FindAsync(id);
        }

        /// <summary>
        /// Add an API Resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task Post([FromBody]ApiResourceViewModel resource)
        {
            await _service.AddAsync(resource);
        }

        /// <summary>
        /// Update an API Resource
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody]ApiResourceViewModel resource)
        {
            await _service.UpdateAsync(id, resource);
        }

        /// <summary>
        /// Delete an API Resource
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var resource = await _service.FindAsync(id);
            if (resource == null)
            {
                throw new NotFoundException("Identity resource does not exist");
            }
            await _service.DeleteAsync();
        }
    }
}
