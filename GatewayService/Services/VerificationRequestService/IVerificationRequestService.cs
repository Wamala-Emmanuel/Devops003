using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GatewayService.DTOs;

namespace GatewayService.Services
{
    public interface IVerificationRequestService
    {
        /// <summary>
        /// Returns a paginated list of requests
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SearchResponse> GetRequestsAsync(SearchRequest request);

        /// <summary>
        /// Returns either a request with the Id or null if it doesnot exist
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        Task<RequestViewModel?> GetRequestStatusAsync(Guid requestId);
    }
}
