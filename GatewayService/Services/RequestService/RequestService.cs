using System;
using System.Threading.Tasks;
using GatewayService.DTOs;
using GatewayService.Helpers;
using GatewayService.Models;
using GatewayService.Repositories.Contracts;
using GatewayService.Services.Nira;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Request = GatewayService.Models.Request;

namespace GatewayService.Services
{
#nullable enable
    public class RequestService : IRequestService
    {
        private readonly INiraService _niraService;
        private readonly ITokenUtil _tokenUtil;
        private readonly IRequestRepository _repository;
        private readonly ILogger<RequestService> _logger;
        private readonly IBackgroundJobClient _backgroundJob;
        private readonly ICredentialService _credentialService;
        private readonly bool _useDatabaseCredentials; 
        private readonly double _offset;
        private readonly IOptions<AuthServiceSettings> _authOptions;
        private readonly IOptions<SubscriptionSettings> _subOptions;

        public RequestService(IRequestRepository repository, INiraService niraService, 
            IOptions<NiraSettings> niraOptions,IOptions<AuthServiceSettings> authOptions,
            IOptions<SubscriptionSettings> subOptions,
            ILogger<RequestService> logger, IBackgroundJobClient backgroundJob,
            ITokenUtil tokenUtil, ICredentialService credentialService)
        {
            var niraSettings = niraOptions.Value;

            _authOptions = authOptions;
            _tokenUtil = tokenUtil;
            _repository = repository;
            _niraService = niraService;
            _logger = logger;
            _useDatabaseCredentials = niraSettings.CredentialConfig.UseDatabaseCredentials;
            _offset = niraSettings.NiraDateTimeConfig.Offset;
            _backgroundJob = backgroundJob;
            _credentialService = credentialService;
            _subOptions = subOptions;
        }

        /// <summary>
        /// Process the request and get the nira response
        /// </summary>
        /// <param name="request"></param>      
        /// <returns></returns>
        public async Task<Guid> Process(NationalIdVerificationRequest request, HttpRequest httpRequest)
        {
            _logger.LogInformation("Processing the verification request... with traceID: {TraceID}", httpRequest.HttpContext.TraceIdentifier);

            var userInfo = _tokenUtil.GetTokenClaims(httpRequest);

            if (_tokenUtil.ClaimExists(userInfo, _authOptions.Value.AuthClaims.RoleClaim))
            {
                userInfo = await _tokenUtil.GetUserInfo(httpRequest);
            }

            var sub = _tokenUtil.TryGetClaimValue(userInfo, _authOptions.Value.AuthClaims.SubClaim);

            var userName = _tokenUtil.TryGetClaimValue(userInfo, _authOptions.Value.AuthClaims.NameClaim);
            
            var userEmail = _tokenUtil.TryGetClaimValue(userInfo, _authOptions.Value.AuthClaims.GivenNameClaim);

            var newRequest = new Request
            {
                Surname = request.Surname,
                GivenNames = request.GivenNames,
                DateOfBirth = request.DateOfBirth,
                CardNumber = request.CardNumber,
                Nin = request.Nin,
                Initiator = userName ?? null,
                InitiatorId = Guid.TryParse(sub, out Guid userId) ? userId : (Guid?)null,
                InitiatorEmail = userEmail ?? null,
                ReceivedAt = DateTime.UtcNow.AddHours(_offset),
                RequestStatus = RequestStatus.Pending,
            };

            var subscriptionExists = httpRequest.Headers.TryGetValue(
                _subOptions.Value.HeaderName, out Microsoft.Extensions.Primitives.StringValues subscriptionKey);

            if (subscriptionExists)
            {
                newRequest.Initiator = subscriptionKey;
            }

            await _repository.AddAsync(newRequest);

            if (subscriptionExists)
            {
                _logger.LogInformation(
                    "New verification request with request Id {RequestId} with subscriptionKey: {SubscriptionKey} and with traceID: {TraceID}", newRequest.Id, subscriptionKey, httpRequest.HttpContext.TraceIdentifier);
            }
            else
            {
                _logger.LogInformation(
                    "New verification request with request Id {RequestId} made by: {Initiator} with traceID: {TraceID}", newRequest.Id, newRequest.Initiator, httpRequest.HttpContext.TraceIdentifier);
            }

            var jobId = _backgroundJob.Enqueue(() => _niraService.SendRequest(newRequest.Id));

            if (_useDatabaseCredentials)
            {
                var latestCredentials = await _credentialService.GetLatestCredentialsAsync();

                if (string.IsNullOrWhiteSpace(latestCredentials.JobId) && !latestCredentials.ExpiresOn.HasValue)
                {
                    _logger.LogInformation("Scheduling renew password job for {NiraUserName} the latest credentials", latestCredentials.Username);

                    _backgroundJob.ContinueJobWith(jobId,
                        () => _credentialService.SchedulePasswordRenewalJobAsync(newRequest.Id));
                }
                else
                {
                    _logger.LogInformation("The latest credentials {NiraUserName} have a renew password job scheduled already.", latestCredentials);
                }
            }

            return newRequest.Id;
        }
    }
}
