using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer4.EntityFramework.Entities;
using Laboremus_AuthorizationService.Core.Extensions;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Services.IdentityResources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Laboremus_AuthorizationService.Controllers
{
    /// <inheritdoc />
    [Route("api/identityResources")]
    public class IdentityResourcesController : BaseController
    {
        private readonly IIdentityResourceService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// 
        /// </summary>  
        /// <param name="service"></param>
        /// <param name="mapper"></param>
        public IdentityResourcesController(IIdentityResourceService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all Identity Resources
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<IdentityResourceViewModel>> Get()
        {
            var resources = await _service.GetAll().ToListAsync();
            return _mapper.Map<IEnumerable<IdentityResourceViewModel>>(resources);
        }

        /// <summary>
        /// Get an Identity Resource by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IdentityResourceViewModel> Get(int id)
        {
            var resource = await _service.FindBy(q => q.Id == id).FirstOrDefaultAsync();
            return _mapper.Map<IdentityResourceViewModel>(resource);
        }
        
        /// <summary>
        /// Add an Identity Resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task Post([FromBody]IdentityResourceViewModel resource)
        {
            await _service.AddAsync(resource);
        }
        
        /// <summary>
        /// Update an Identity Resource
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody]IdentityResourceViewModel resource)
        {
            await _service.UpdateAsync(id, resource);
        }
        
        /// <summary>
        /// Delete an Identity Resource
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
