using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GatewayService.DTOs;
using GatewayService.DTOs.NitaCredentials;
using GatewayService.Helpers;
using GatewayService.Models;
using GatewayService.Repositories.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GatewayService.Services.Nita.NitaCredentialService
{
    public class NitaCredentialService : INitaCredentialService
    {
        private readonly INitaCredentialRepository _nitaCredentialRepository;
        private readonly IOptions<NitaSettings> _nitaOptions;
        private readonly ILogger<NitaCredentialService> _logger;

        public NitaCredentialService(INitaCredentialRepository nitaCredentialRepository,
            IOptions<NitaSettings> nitaOptions, ILogger<NitaCredentialService> logger)
        {
            _nitaCredentialRepository = nitaCredentialRepository;
            _nitaOptions = nitaOptions;
            _logger = logger;
        }

        public async Task<bool> AreNitaCredentialsSet()
        {
            var total = await _nitaCredentialRepository.GetCountAsync();
            return total > 0;
        }

        public async Task<List<NitaCredential>> GetAllNitaCredentialsAsync()
        {
            return await _nitaCredentialRepository.GetAllAsync();
        }

        public async Task<NitaCredentialResponse> GetCurrentNitaCredentialsAsync()
        {
            var credentials = await _nitaCredentialRepository.GetLatestAsync();

            if (credentials == null)
            {
                _logger.LogInformation("Please set the latest NITA credentials");

                throw new NotFoundException("Failed to find the latest NITA credentials.");
            }

            return GetNitaCredentialResponse(credentials);
        }

        public async Task<NitaCredential> GetLatestNitaCredentialsAsync()
        {
            var latestCredentials = await _nitaCredentialRepository.GetLatestAsync();

            if (latestCredentials == null)
            {
                _logger.LogInformation("Please set the latest NITA credentials");

                throw new NotFoundException("Failed to find the latest NITA credentials.");
            }

            return latestCredentials;
        }

        public async Task<NitaCredentialResponse> SetNitaCredentialsAsync(NitaCredentialRequest request)
        {
            var nitaSettings = _nitaOptions.Value;

            var newCredentials = new NitaCredential
            {
                CreatedOn = DateTime.UtcNow.AddHours(nitaSettings.Offset),
                ClientKey = request.ClientKey,
                ClientSecret = request.ClientSecret,
            };

            var response = await _nitaCredentialRepository.AddAsync(newCredentials);

            _logger.LogInformation("Set NITA credentials for {ClientKey}", response.ClientKey);

            var updatedCredentials = GetNitaCredentialResponse(response);

            return updatedCredentials;
        }

        public async Task<NitaCredentialResponse> UpdateNitaCredentialsAsync(Guid id, NitaCredentialRequest request)
        {

            var foundCredential = await _nitaCredentialRepository.FindAsync(id);

            if (foundCredential == null)
            {
                _logger.LogWarning("Failed to find NITA credentials with id: {NitaCredentialId}.", id);
                throw new NotFoundException($"Failed to find NITA credentials with id: '{id}'.");
            }

            _logger.LogInformation("Retrieved a  NITA credentials with id: {NitaCredentialId}.", id);

            var nitaSettings = _nitaOptions.Value;

            foundCredential.ClientKey = request.ClientKey;
            foundCredential.ClientSecret = request.ClientSecret;
            foundCredential.UpdatedOn = DateTime.UtcNow.AddHours(nitaSettings.Offset);

            var response = await _nitaCredentialRepository.UpdateAsync(foundCredential);

            _logger.LogInformation("Updated NITA credentials for {ClientKey}", response.ClientKey);

            var updatedCredentials = GetNitaCredentialResponse(response);

            return updatedCredentials;
        }

        private NitaCredentialResponse GetNitaCredentialResponse(NitaCredential credential)
        {
            return new NitaCredentialResponse
            {
                Id = credential.Id,
                ClientKey = credential.ClientKey,
                CreatedOn = credential.CreatedOn,
                UpdatedOn = credential.UpdatedOn
            };
        }
    }
}
