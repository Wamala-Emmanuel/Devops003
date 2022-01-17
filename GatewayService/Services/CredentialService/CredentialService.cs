using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GatewayService.DTOs;
using GatewayService.DTOs.Credentials;
using GatewayService.Helpers;
using GatewayService.Models;
using GatewayService.Repositories.Contracts;
using GatewayService.Services.Nira;
using GatewayService.Services.NotifierService;
using Hangfire;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NiraWebService;

namespace GatewayService.Services
{
    public class CredentialService : ICredentialService
    {
        private readonly IBackgroundJobClient _backgroundJob;
        private readonly ICredentialRepository _credentialRepository;
        private readonly IRequestRepository _requestRepository;
        private readonly IOptions<NiraSettings> _niraOptions;
        private readonly ILogger<CredentialService> _logger;
        private readonly IBackgroundJobWrapper _backgroundJobWrapper;
        private readonly INiraService _niraService;
        private readonly INotifierService _notifierService;

        public CredentialService(INiraService niraService, ICredentialRepository repository, IRequestRepository requestRepository,
            IOptions<NiraSettings> niraOptions, IBackgroundJobClient backgroundJob, ILogger<CredentialService> logger,
            INotifierService notifierService, IBackgroundJobWrapper backgroundJobWrapper)
        {
            _backgroundJob = backgroundJob;
            _logger = logger;
            _backgroundJobWrapper = backgroundJobWrapper;
            _niraService = niraService;
            _credentialRepository = repository;
            _requestRepository = requestRepository;
            _niraOptions = niraOptions;
            _notifierService = notifierService;
        }

        public async Task SchedulePasswordRenewalJobAsync(Guid requestId)
        {

            var niraSettings = _niraOptions.Value;

            var verificationRequest = await _requestRepository.FindAsync(requestId);

            if (verificationRequest == null)
            {
                _logger.LogError("Failed to retrieve request with request Id: {RequestId}", requestId);
                throw new NotFoundException($"Failed to find request with Id: {requestId}");
            }

            _logger.LogInformation("Retrieved request with request Id: {RequestId}", requestId);


            var resultJson = JsonConvert.DeserializeObject<VerificationResult>(verificationRequest.Result ?? "{}",
                         new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            if (!resultJson.PasswordDaysLeft.HasValue)
            {
                _logger.LogInformation("The request with Id: {RequestId} was either not completed or has no password days left", requestId);
                return;
            }

            var latestCredentials = await _credentialRepository.GetLatestAsync();

            if (latestCredentials == null)
            {
                throw new NotFoundException($"Failed to find the latest credentials.");
            }

            _logger.LogInformation("Retrieved the latest credentials for {NiraUsername}", latestCredentials.Username);

            var timeSpanDelay = TimeSpan.Zero;
            
            if (resultJson.PasswordDaysLeft <= niraSettings.CredentialConfig.PasswordDaysLimit)
            {
                timeSpanDelay = TimeSpan.FromMinutes(1);
                _logger.LogInformation("The renew password job will be scheduled to run after one minute");
            }
            else
            {
                var daysLeft = resultJson.PasswordDaysLeft.Value - niraSettings.CredentialConfig.PasswordDaysLimit;
                
                timeSpanDelay = TimeSpan.FromDays(daysLeft);

                _logger.LogInformation("The renew password job will be scheduled to run after {DaysToRun} days", timeSpanDelay.Days);
            }

            var jobId =
                _backgroundJob.Schedule(() => _niraService.RenewPasswordAsync(latestCredentials.Id),
                    timeSpanDelay);

            //updating the database, to put the password days left.
            latestCredentials.ExpiresOn = DateTime.UtcNow.Date
                .AddDays(resultJson.PasswordDaysLeft.Value);
            latestCredentials.JobId = jobId;

            await _credentialRepository.UpdateAsync(latestCredentials);

            _logger.LogInformation("The latest credentials for {NiraUsername} have been updated in the database", latestCredentials.Username);

            _logger.LogInformation("The renew password job has been scheduled");
        }

        public async Task<List<Credential>> GetAllCredentialsAsync()
        {
            return await _credentialRepository.GetAllAsync();
        }

        public async Task<CredentialResponse> GetCurrentCredentialsAsync()
        {
            var niraSettings = _niraOptions.Value;

            if (!niraSettings.CredentialConfig.UseDatabaseCredentials)
            {
                return new CredentialResponse
                {
                    Username = niraSettings.CredentialConfig.Username,
                    
                };
            }

            var credentials = await GetLatestCredentialsAsync();

            return GetCredentialResponse(credentials);
        }

        public async Task<CredentialResponse> SetCredentialsAsync(CredentialRequest request)
        {
            var niraSettings = _niraOptions.Value;

            //find the current credentials and delete the BJ job if it exists
            var currentCredentials = await _credentialRepository.GetLatestAsync();
            if (currentCredentials?.JobId != null)
            {
                _backgroundJobWrapper.DeleteJob(currentCredentials.JobId);
            }

            var newCredentials = new Credential
            {
                CreatedOn = DateTime.UtcNow.AddHours(niraSettings.NiraDateTimeConfig.Offset),
                Password = request.Password,
                Username = request.Username,
            };

            var response = await _credentialRepository.AddAsync(newCredentials);

            _logger.LogInformation("Set credentials for {NiraUsername}", response.Username);

            var updatedCredentials = GetCredentialResponse(response);

            _notifierService.PublishCredentials(updatedCredentials);

            return updatedCredentials;
        }

        public async Task<Credential> GetLatestCredentialsAsync()
        {
            var latestCredentials = await _credentialRepository.GetLatestAsync();

            if (latestCredentials == null)
            {
                _logger.LogInformation("Please set the latest credentials");

                throw new NotFoundException($"Failed to find the latest credentials.");
            }

            return latestCredentials;
        }

        public bool AreConfigCredentialsSet()
        {
            var niraSettings = _niraOptions.Value;

            return !string.IsNullOrEmpty(niraSettings.CredentialConfig.Username)
                   && !string.IsNullOrEmpty(niraSettings.CredentialConfig.Password);
        }

        public Task<bool> AreDatabaseCredentialsSet()
        {
            return _credentialRepository.AnyActiveCredentials();
        }

        private CredentialResponse GetCredentialResponse(Credential credential)
        {
            return new CredentialResponse
            {
                Id = credential.Id,
                Username = credential.Username,
                CreatedOn = credential.CreatedOn,
                ExpiresOn = credential.ExpiresOn
            };
        }
    }
}
