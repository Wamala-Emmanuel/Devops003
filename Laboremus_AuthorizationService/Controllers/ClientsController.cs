using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.EntityFramework.Entities;
using Laboremus_AuthorizationService.Core.Extensions;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Services.Clients;

namespace Laboremus_AuthorizationService.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Handles all actions about the client
    /// </summary>
    [Route("api/clients")]
    public class ClientsController : BaseController
    {
        private readonly IClientService _service;
        private readonly IMapper _mapper;

        /// <param name="service"></param>
        /// <param name="mapper"></param>
        public ClientsController(IClientService service,IMapper mapper )
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all clients
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<ClientViewModel>> Get()
        {
            var clients = await _service.GetAll()
                .Include(q => q.RedirectUris)
                .Include(q => q.PostLogoutRedirectUris)
                .Include(q => q.AllowedGrantTypes)
                .Include(q => q.AllowedScopes)
                .Include(q => q.Claims)
                .Include(q => q.IdentityProviderRestrictions)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ClientViewModel>>(clients);
        }

        /// <summary>
        /// Get client by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ClientViewModel> Get([FromRoute] int id)
        {
            var client = await _service.FindBy(q => q.Id == id)
                .Include(q => q.RedirectUris)
                .Include(q => q.PostLogoutRedirectUris)
                .Include(q => q.AllowedGrantTypes)
                .Include(q => q.AllowedScopes)
                .Include(q => q.Claims)
                .Include(q => q.IdentityProviderRestrictions)
                .FirstOrDefaultAsync();

            return _mapper.Map<ClientViewModel>(client);
        }

        /// <summary>
        /// Update client
        /// </summary>
        /// <param name="id"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task Put([FromRoute] int id, [FromBody] ClientViewModel client)
        {
            await _service.UpdateAsync(id, client);
        }

        /// <summary>
        /// Add a client
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task Post([FromBody] ClientViewModel client)
        {
            await _service.AddAsync(client);
        }

        /// <summary>
        /// Delete a client
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task DeleteClient([FromRoute] int id)
        {
            var client = await _service.FindAsync(id);
            if (client == null)
            {
                throw new NotFoundException("Client not found");
            }
            await _service.DeleteAsync(client);
        }

    }
}