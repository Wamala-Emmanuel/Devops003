using Laboremus_AuthorizationService.Models;
using Laboremus_AuthorizationService.Services.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.ViewModels;
using Newtonsoft.Json;

namespace Laboremus_AuthorizationService.Controllers
{
    /// <inheritdoc />
    [Route("api/user/customClaims")]
    public class CustomClaimsController : BaseController
    {
        private readonly ICustomClaimsService _service;

        public CustomClaimsController(ICustomClaimsService service)
        {
            _service = service;
        }
        /// <summary>
        /// Get all customClaims
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<CustomClaimViewModel>> Get([FromQuery]string id)
        {
            var data = await _service.GetClaims(id);
            return data.Select(it => new CustomClaimViewModel
            {
                Id = it.Id,
                Claims= JsonConvert.DeserializeObject<List<ClaimViewModel>>(it.ClaimsData)
            }).ToList();
        }

        /// <summary>
        /// Update customClaim
        /// </summary>
        /// <param name="customClaim"></param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(OkResult), 200)]
        public async Task Put([FromBody] CustomClaimViewModel customClaim)
        {
            await _service.EditClaims(new CustomClaim
            {
                Id = customClaim.Id,
                ClaimsData = JsonConvert.SerializeObject(customClaim.Claims)
            });
        }

        /// <summary>
        /// Add a customClaim
        /// </summary>
        /// <param name="customClaim"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(OkResult), 200)]
        public async Task Post([FromBody] CustomClaimViewModel customClaim)
        {
            await _service.AddClaims(new List<CustomClaim> { new CustomClaim
            {
                Id = customClaim.Id,
                ClaimsData = JsonConvert.SerializeObject(customClaim.Claims)
            } });
        }

        /// <summary>
        /// Add multiple claims
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("/many")]
        [ProducesResponseType(typeof(OkResult), 200)]
        public async Task PostMany([FromBody] List<CustomClaimViewModel> data)
        {
            await _service.AddClaims(data.Select(it=> new CustomClaim
            {
                Id = it.Id,
                ClaimsData = JsonConvert.SerializeObject(it.Claims)
            }).ToList());
        }
        /// <summary>
        /// Delete a customClaim
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(OkResult), 200)]
        public async Task DeleteCustomClaim([FromRoute] string id)
        {
            await _service.DeleteClaims(id);
        }
    }
}
