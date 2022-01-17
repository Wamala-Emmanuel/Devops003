using System;
using System.Threading.Tasks;
using GatewayService.DTOs;
using GatewayService.Helpers;
using GatewayService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace GatewayService.Controllers
{
    [Route("api/requests")]
    [ApiVersion("1.0")]
    [SwaggerTag("ID verification requests endpoints.")]
    public class RequestsController : BaseController
    {
        private readonly IVerificationRequestService _verificationRequestService;
        private readonly IRequestService _requestService;

        private readonly ILogger<VerificationRequestService> _logger;

        public RequestsController(IVerificationRequestService service,
           IRequestService requestService, ILogger<VerificationRequestService> logger)
        {
            _logger = logger;
            _requestService = requestService;
            _verificationRequestService = service;
        }

        /// <summary>
        /// Get Request Details
        /// </summary>
        /// <remarks>Endpoint for getting details of a specific request by unique id.</remarks>
        /// <param name="id">id</param>
        /// <example>8754b7cb-d0fc-4499-8a1a-ebfb721cf0fc</example>
        /// <returns></returns>
        [SwaggerResponse(
            StatusCodes.Status200OK, 
            "The details of a given request", 
            typeof(RequestViewModel))]
        [HttpGet("{id}", Name = "GetRequestById")]
        public async Task<IActionResult> GetRequestStatus([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Invalid request Id");
            var result = await _verificationRequestService.GetRequestStatusAsync(id);
            if (result is null)
            {
                _logger.LogInformation("Failed to find request with request Id: {RequestId}", id);
                return NotFound();
            }
            _logger.LogInformation("Retrieved request with request Id: {request Id}", id);
            return Ok(result);
        }

        /// <summary>
        /// Search Requests
        /// </summary>
        /// <remarks>In case your system would like to search the local database of the Financial Institution Application
        /// for existing requests before sending a new request to NIRA, you can use the following end point.</remarks>
        /// <param name="request"></param>
        /// <example></example>
        /// <returns></returns>
        [SwaggerResponse(
            StatusCodes.Status200OK, "Operation successful",
            typeof(SearchResponse))]
        [HttpGet ( Name = "SearchRequests")]
        public async Task<IActionResult> SearchRequestsAsync(
            [FromQuery, SwaggerParameter("Search parameters", Required = false)] SearchRequest request)
        {

            var result = await _verificationRequestService.GetRequestsAsync(request);

            return Ok(result);
        }

        /// <summary>
        /// Make Verification Request
        /// </summary>
        /// <remarks>
        /// Accepts requests for verifying Ugandan National ID details. All requests are queued and processed as soon as possible.
        /// </remarks>
        /// <returns></returns>
        [HttpPost (Name = "AddRequest")]
        [SwaggerResponse(
            statusCode: StatusCodes.Status202Accepted,
            description: "The request has been received and accepted for processing.",
            typeof(VerificationPendingResponse))]
        [ValidateModel]
        public async Task<IActionResult> Create(
            [FromBody, SwaggerRequestBody("The verification request payload", Required = true)] NationalIdVerificationRequest request)
        {
            _logger.LogInformation("New Verification request.");
            var apiVersion = HttpContext.GetRequestedApiVersion();

            var requestId = await _requestService.Process(request, HttpContext.Request);

            return AcceptedAtAction(nameof(Accepted), new VerificationPendingResponse(requestId, apiVersion));
        }

    }
}