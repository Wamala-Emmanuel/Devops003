using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GatewayService.DTOs;
using GatewayService.Helpers;
using GatewayService.Helpers.Nira;
using GatewayService.Models;
using GatewayService.Repositories.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GatewayService.Services.BillingService
{
    public class BillingService : IBillingService
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IConfiguration _config;
        private readonly ILogger<IBillingService> _logger;
        private readonly ITokenUtil _tokenUtil;
        private readonly double _offset;
        private readonly List<string> _billableErrorCodes;
        private readonly string _participantIdClaim = "participant_id";

        public BillingService (IRequestRepository requestRepository, IConfiguration config, ILogger<IBillingService> logger, ITokenUtil tokenUtil)
        {
            var settings = config.GetNiraSettings();

            _requestRepository = requestRepository;
            _config = config;
            _logger = logger;
            _offset = settings.NiraDateTimeConfig.Offset;
            _billableErrorCodes = settings.BillableErrorCodes;
            _tokenUtil = tokenUtil;
        }

        /// <summary>
        /// Notifies the billing serivice when a successful response is received from NIRA
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task CreateNotificationAsync(BillingRequest request)
        {
         
            var serviceUrl = _config.GetBillingServiceSettings().Url;

            var apiClient = new ApiClient(await _tokenUtil.GetTokenAsync(), new List<(string, string)>(), _logger);

            var resp = await apiClient.PostAsync(serviceUrl, request);

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation(
                     "Notification sent request with ID: {RequestReferenceId} company: {RequestCompanyId} response:{ResponseData}", request.ReferenceId, request.CompanyId, resp.Data);
            }
            else
            {
                _logger.LogWarning(
                   "Notification failed request with ID: {RequestReferenceId} company: {RequestCompanyId} statusCode:{ResponseStatusCode} Message:{ResponseMessage}", request.ReferenceId, request.CompanyId, resp.Message);
            }
        }

        public async Task UpdateBilling(Guid requestId)
        {
            var request = await _requestRepository.FindAsync(requestId);

            if (request == null)
            {
                _logger.LogInformation("Failed to find request with request Id {RequestId}", requestId);
                throw new NotFoundException($"Failed to find request with Id {requestId}.");
            }

            _logger.LogInformation("Retrieved request with request Id {RequestId}", requestId);

            if (request.RequestStatus != RequestStatus.Completed)
            {
                _logger.LogInformation("Request with request Id {RequestId} isnot billable", requestId);

                return;
            }
            
            var resultJson = JsonConvert.DeserializeObject<VerificationResult>(request.Result ?? "{}",
                         new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var token = await _tokenUtil.GetTokenAsync();

            var claims = _tokenUtil.GetTokenClaims(token);

            var participantId = _tokenUtil.TryGetClaimValue(claims, _participantIdClaim);

            request.ParticipantId = Guid.TryParse(participantId, out Guid id) ? id : (Guid?)null;

            var billingRequest = new BillingRequest
            {
                ReferenceId = request.Id,
                CompanyId = request.ParticipantId ?? null,
                MetaData = new NiraMetaData
                {
                    IsMatching = resultJson.MatchingStatus,
                    IsCardValid = resultJson.CardStatus,
                    NiraResponseStatus = resultJson.Status,
                    CardNumber = NiraUtils.MaskCardDetails(request.CardNumber, charsToShow: 5),
                    Initiator = request.Initiator,
                    SubmittedAt = request.SubmittedAt ?? null,
                    ReceivedFromNira = request.ReceivedFromNira ?? null,
                    Nin = NiraUtils.MaskCardDetails(request.Nin, charsToShow: 6),
                    Gender = NiraUtils.GetGenderFromNin(request.Nin),
                }
            };

            if (request.DateOfBirth.HasValue)
            {
                billingRequest.MetaData.DateOfBirth = request.DateOfBirth.Value;
            };

            var serviceUrl = _config.GetBillingServiceSettings().Url;

            var apiClient = new ApiClient(token, new List<(string, string)>(), _logger);

            var resp = await apiClient.PostAsync(serviceUrl, billingRequest);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientFriendlyException("Failed to connect to Billing.");                
            }

            _logger.LogInformation(
                "Billing info sent with request with ID: {ReferenceId} company ID: {CompanyId} response:{Response}", billingRequest.ReferenceId, billingRequest.CompanyId, resp.Data);

            request.BillingUpdated = DateTime.UtcNow.AddHours(_offset);

            await _requestRepository.UpdateAsync(request);

            _logger.LogInformation("Updated details of the request with id:{RequestId}", request.Id);
        }
    }
}
