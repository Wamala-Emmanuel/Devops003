using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Services.Clients.Claims;
using Laboremus_AuthorizationService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Laboremus_AuthorizationService.Controllers
{
    /// <inheritdoc />
    [Route("api/Client/claims")]
    public class ClientClaimsController : BaseController
    {
        private readonly IClientClaimService _service;

        public ClientClaimsController(IClientClaimService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IEnumerable<ClientClaimViewModel>> Get()
        {
            return await _service.GetAll()
                .Select(s => new ClientClaimViewModel
                {
                    ClientId = s.Client.Id,
                    Type = s.Type,
                    Value = s.Value
                }).ToListAsync();
        }

        [HttpGet("{id}", Name = "Get")]
        public async Task<ClientClaimViewModel> Get(int id)
        {
            var claim = await _service.FirstOrDefaultAsync(q => q.Id == id);
            if (claim == null)
            {
                throw new NullReferenceException("Claim not found");
            }

            return new ClientClaimViewModel
            {
                Type = claim.Type,
                Value = claim.Value,
                ClientId = claim.Client.Id
            };
        }

        [HttpPost]
        public async Task Post([FromBody]ClientClaimViewModel model)
        {
            await _service.AddAsync(model);
        }

        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody]ClientClaimViewModel model)
        {
            await _service.UpdateAsync(id, model);
        }

        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var claim = await _service.FindAsync(id);
            if (claim != null)
            {
                await _service.DeleteAsync(claim);
            }
        }
    }
}
