using System;
using System.Globalization;
using System.Threading.Tasks;
using GatewayService.DTOs;
using GatewayService.Helpers;
using GatewayService.Helpers.Nira;
using GatewayService.Models;
using GatewayService.Repositories.Contracts;
using GatewayService.Services.Nita.Json;
using GatewayService.Services.NotifierService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NiraWebService;

namespace GatewayService.Services.Nira
{
    public class NiraService : INiraService
    {
        private readonly string _username;
        private readonly string _password; 
        private readonly string _culture;
        private readonly double _offset;
        private readonly string _dateFormat;
        private readonly bool _useDatabaseCredentials;
        private readonly ILogger<NiraService> _logger;
        private readonly ICredentialRepository _credentialRepository;
        private readonly IRequestRepository _requestRepository;
        private readonly INiraCoreService _niraCoreService;
        private readonly INitaJsonService _nitaJsonService;
        private readonly IOptions<VerificationSettings> _verificationOptions;
        private readonly INotifierService _notifierService;

        public NiraService(INiraCoreService niraCoreService, ICredentialRepository credentialRepository, 
            IRequestRepository requestRepository, IConfiguration configuration,
            INitaJsonService nitaJsonService, IOptions<VerificationSettings> verificationOptions,
            INotifierService notifierService, ILogger<NiraService> logger)
        {
            var settings = configuration.GetNiraSettings();
            _username = settings.CredentialConfig.Username;
            _password = settings.CredentialConfig.Password; 
            _offset = settings.NiraDateTimeConfig.Offset;
            _culture = settings.NiraDateTimeConfig.Culture;
            _logger = logger;
            _dateFormat = settings.NiraDateTimeConfig.NiraDateFormat;
            _useDatabaseCredentials = settings.CredentialConfig.UseDatabaseCredentials;
            _credentialRepository = credentialRepository;
            _requestRepository = requestRepository;
            _niraCoreService = niraCoreService;
            _notifierService = notifierService;
            _nitaJsonService = nitaJsonService;
            _verificationOptions = verificationOptions;
        }

        public async Task SendRequest(Guid requestId)
        {
            var request = await _requestRepository.FindAsync(requestId);
            
            if (request is null)
            {
                throw new NotFoundException($"Failed to find request with Id: {requestId}");
            }

            _logger.LogInformation("Retrieved request with request Id: {RequestId}", requestId);

            var niraRequest = new verifyPersonInformationRequest
            {
                documentId = request.CardNumber,
                nationalId = request.Nin,
                surname = request.Surname,
                givenNames = request.GivenNames,
                dateOfBirth = request.DateOfBirth?.AddHours(_offset).ToString(_dateFormat, new CultureInfo(_culture))
            };

            request.SubmittedAt = DateTime.UtcNow.AddHours(_offset);

            _logger.LogInformation(
                "Verify person information request with id: {RequestId} submitted to NIRA at {SubmittedAt}.", request.Id, request.SubmittedAt);

            string password;
            string username;
            if (_useDatabaseCredentials)
            {
                var latestCredentials = await _credentialRepository.GetLatestAsync();

                if (latestCredentials is null)
                {
                    _logger.LogInformation("Please set credentials");
                    throw new NotFoundException($"Please set credentials.");
                }

                _logger.LogInformation("Retrieved the latest credentials for {niraUsername}.", latestCredentials.Username);

                password = latestCredentials.Password;

                username = latestCredentials.Username;

                _logger.LogDebug("Using credentials from the database.");
            }
            else
            {
                password = _password;

                username = _username;

                _logger.LogDebug("Using credentials from app settings.");
            }

            if (_verificationOptions.Value.ConnectionType == ConnectionType.Nira)
            {
                _logger.LogInformation("Using a direct NIRA connection to verify person details.");
                var niraResponse = await _niraCoreService.VerifyPersonInformation(username, password, niraRequest);
                request.Result = JsonConvert.SerializeObject(niraResponse);
            }
            else
            {
                _logger.LogInformation("Using a NITA connection to verify person details.");
                var niraResponse = await _nitaJsonService.MakeJsonRequest(username, password, niraRequest);
                request.Result = JsonConvert.SerializeObject(niraResponse);
            }

            request.ReceivedFromNira = DateTime.UtcNow.AddHours(_offset);
            request.RequestStatus = RequestStatus.Completed;

            _logger.LogInformation(
                "Got a nira response for request with id: {RequestId} at {ReceivedFromNira} with a result[{Result}].", request.Id, request.ReceivedFromNira, request.Result);

            _logger.LogTrace("Serialized nira response: {Result}", request.Result);

            var updatedRequest =  await _requestRepository.UpdateAsync(request);

            _notifierService.PublishRequest(updatedRequest);

            _logger.LogInformation("Updated details of the request with id:{RequestId}", request.Id);
        }

        public async Task RenewPasswordAsync(Guid credentialsId)
        {
            var newPassword = NiraUtils.GetNiraPassword();
            var generatedPassword = new changePasswordRequest
            {
                newPassword = newPassword
            };

            var credentials = await _credentialRepository.FindAsync(credentialsId);

            _logger.LogDebug("New password generated: {NewPassword}",  newPassword);

            var response = new ChangePasswordResponse();

            if (_verificationOptions.Value.ConnectionType == ConnectionType.Nira)
            {
                _logger.LogInformation("Using a direct NIRA connection to change password for {Username}", credentials.Username);
                response = await _niraCoreService
                    .ChangePasswordAsync(credentials.Username, credentials.Password, generatedPassword);
            }
            else
            {
                _logger.LogInformation("Using a NITA connection to change password for {Username}", credentials.Username);
                response = await _nitaJsonService
                    .MakeJsonRequest(credentials.Username, credentials.Password, generatedPassword);
            }

            if (response.IsError)
            {
                var exception = new ApplicationException(
                    $"Failed to set credentials for {credentials.Username}: {Environment.NewLine} {response.Error.Code} - {response.Error.Message} ");
                _logger.LogError(exception, "An error occured while setting new credentials for {CredentialsUsername}", credentials.Username);

                throw exception;
            }

            var newCredentials = new Credential
            {
                CreatedOn = DateTime.UtcNow.AddHours(_offset),
                Password = generatedPassword.newPassword,
                Username = credentials.Username,
            };

            await _credentialRepository.AddAsync(newCredentials);

            _logger.LogInformation("The new credentials for {NiraUsername} have been saved to the database", credentials.Username);

            _logger.LogDebug("The new password for {NiraUsername} is {NewPassword}", credentials.Username, generatedPassword.newPassword);
        }
    }
}