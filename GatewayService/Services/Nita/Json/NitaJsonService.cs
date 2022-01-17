using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GatewayService.DTOs;
using GatewayService.DTOs.NitaCredentials;
using GatewayService.Helpers;
using GatewayService.Helpers.Nira;
using GatewayService.Repositories.Contracts;
using GatewayService.Services.Nira;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NiraWebService;

namespace GatewayService.Services.Nita.Json
{
    public class NitaJsonService : INitaJsonService
    {
        private readonly INitaCredentialRepository _nitaCredentialRepository;
        private readonly IOptions<NitaSettings> _nitaOptions;
        private readonly ILogger<NitaJsonService> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public NitaJsonService(INitaCredentialRepository nitaCredentialRepository, 
            IOptions<NitaSettings> nitaOptions, 
            IHttpClientFactory clientFactory,
            ILogger<NitaJsonService> logger)
        {
            _clientFactory = clientFactory;
            _nitaCredentialRepository = nitaCredentialRepository;
            _nitaOptions = nitaOptions;
            _logger = logger;
        }

        public async Task<string> GetAccessToken()
        {
            var credentials = await _nitaCredentialRepository.GetLatestAsync();

            if (credentials == null)
            {
                _logger.LogInformation("Please set the NITA credentials");

                throw new NotFoundException("Failed to find the NITA credentials.");
            }

            // create bytes array for the consumer key and consumer secret keys in the format consumer-key:consumer-secret
            var combinedStringArray = $"{credentials.ClientKey}:{credentials.ClientSecret}".GetByteArrayFromString();
            // encode the combined string using base64
            var encodedCombinedString = NiraUtils.ConvertToBase64(combinedStringArray);

            var tokenUrl = $"{_nitaOptions.Value.Host}/{_nitaOptions.Value.Segments.TokenSegment}";
            
            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            request.Headers.Add(HttpRequestHeader.Authorization.ToString(), $"Basic {encodedCombinedString}");

            var bodyContent = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            };

            var formDataContent = new FormUrlEncodedContent(bodyContent);

            request.Content = formDataContent;

            var client = _clientFactory.CreateClient("nitahub");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                var token = JsonConvert.DeserializeObject<NitaTokenResponse>(responseString);
                _logger.LogInformation("Retrieved NITA access token");

                return token.AccessToken;
            }
            else
            {
                _logger.LogWarning(
                   "Failed to get a NITA access token statusCode:{ResponseStatusCode} Message:{ResponseRequestMessage}", response.StatusCode, response.RequestMessage);

                throw new NotFoundException($"Failed to get a NITA access token for {credentials.ClientKey}.");
            }
        }

        public async Task<PersonInfoVerificationResponse> MakeJsonRequest(string username, string password, verifyPersonInformationRequest verificationRequest)
        {
            var nitaSettings = _nitaOptions.Value;

            var verifyPersonInformationUrl = $"{nitaSettings.Host}/{nitaSettings.Segments.EnvironmentSegment}/{nitaSettings.ApiVersion}/{nitaSettings.Segments.VerifyPersonInformationSegment}";

            var request = new HttpRequestMessage(HttpMethod.Post, verifyPersonInformationUrl);

            var token = await GetAccessToken();

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            PrepareNitaHeaders(username, password, request);

            var content = new Request
            {
                DocumentId = verificationRequest.documentId,
                DateOfBirth = verificationRequest.dateOfBirth,
                GivenNames = verificationRequest.givenNames,
                Surname = verificationRequest.surname,
                OtherNames = verificationRequest.otherNames,
                NationalId = verificationRequest.nationalId
            };

            var jsonContent = System.Text.Json.JsonSerializer.Serialize(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

            request.Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json);

            var client = _clientFactory.CreateClient("nitahub");

            _logger.LogInformation("Sending verify person information request to NITA");
            var response = await client.SendAsync(request);
            _logger.LogInformation("Received verify person information request from NITA");

            _logger.LogInformation("Received headers for verify person information request from NITA");
            foreach (var item in response.Headers)
            {
                _logger.LogInformation("{HeaderName} - {HeaderValue}", item.Key, item.Value);
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    _logger.LogInformation("The verified person information from NITA is with status Ok");

                    var okResponse = JsonConvert.DeserializeObject<NitaResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

                    // map NITA response to NIRA response for the database record
                    var verificationResponse = new PersonInfoVerificationResponse
                    {
                        IsError = okResponse.Return.TransactionStatus.TransactionStatusTransactionStatus.ToLower() == "error",
                        PasswordDaysLeft = okResponse.Return.TransactionStatus.PasswordDaysLeft.ToString(),
                        Status = okResponse.Return.TransactionStatus.TransactionStatusTransactionStatus,
                        ExecutionCost = okResponse.Return.TransactionStatus.ExecutionCost.ToString(),
                        MatchingStatus = okResponse.Return.MatchingStatus.ToString(),
                        CardStatus = okResponse.Return.CardStatus,
                    };

                    return verificationResponse;

                case HttpStatusCode.BadRequest:
                    _logger.LogInformation("The verified person information from NITA is with an error status");

                    var errorResponse = JsonConvert.DeserializeObject<NitaResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

                    // map NITA response to NIRA response for the database record
                    var errorVerificationResponse = new PersonInfoVerificationResponse
                    {
                        IsError = errorResponse.Return.TransactionStatus.TransactionStatusTransactionStatus.ToLower() == "error",
                        PasswordDaysLeft = errorResponse.Return.TransactionStatus.PasswordDaysLeft.ToString(),
                        Status = errorResponse.Return.TransactionStatus.TransactionStatusTransactionStatus,
                        Error = new ResponseError
                        {
                            Code = errorResponse.Return.TransactionStatus.Error.Code.ToString(),
                            Message = errorResponse.Return.TransactionStatus.Error.Message
                        }
                    };

                    return errorVerificationResponse;

                default:
                    _logger.LogWarning(
                        "Failed to verify person information through NITA statusCode:{ResponseStatusCode} Message:{ResponseRequestMessage}", response.StatusCode, response.RequestMessage);

                    throw new NotFoundException("Failed to verify person information.");
            }
        }

        public async Task<ChangePasswordResponse> MakeJsonRequest(string username, string password, changePasswordRequest passwordRequest)
        {
            var nitaSettings = _nitaOptions.Value;

            var changePasswordUrl = $"{nitaSettings.Host}/{nitaSettings.Segments.EnvironmentSegment}/{nitaSettings.ApiVersion}/{nitaSettings.Segments.ChangePasswordSegment}";
            var certificate = NiraUtils.GetEncryptionCertificate(_nitaOptions.Value.CertificatePath);
            var encryptedPassword = NiraUtils.EncryptWithRSA(certificate, passwordRequest.newPassword);
            var base64Password = encryptedPassword.ToBase64();

            var request = new HttpRequestMessage(HttpMethod.Post, changePasswordUrl);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());
            PrepareNitaHeaders(username, password, request);

            var content = new PasswordRequest
            {
                NewPassword = base64Password
            };

            var jsonContent = System.Text.Json.JsonSerializer.Serialize(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

            request.Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json);

            var client = _clientFactory.CreateClient("nitahub");

            _logger.LogInformation("Sending change Password Request to NITA");
            var response = await client.SendAsync(request);
            _logger.LogInformation("Received change Password Request from NITA");

            _logger.LogInformation("Received headers for change password request from NITA");
            foreach (var item in response.Headers)
            {
                _logger.LogInformation("{HeaderName} - {HeaderValue}", item.Key, item.Value);
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    _logger.LogInformation("Changed Password for {NiraUsername} using the NITA connection", username);

                    var okResponse = JsonConvert.DeserializeObject<NitaResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });


                    // map NITA response to NIRA response for the database record
                    var verificationResponse = new ChangePasswordResponse
                    {
                        IsError = okResponse.Return.TransactionStatus.TransactionStatusTransactionStatus.ToLower() == "error",
                        PasswordDaysLeft = okResponse.Return.TransactionStatus.PasswordDaysLeft.ToString(),
                        Status = okResponse.Return.TransactionStatus.TransactionStatusTransactionStatus,
                        ExecutionCost = okResponse.Return.TransactionStatus.ExecutionCost.ToString(),
                    };

                    return verificationResponse;
                    break;

                case HttpStatusCode.BadRequest:

                    var errorResponse = JsonConvert.DeserializeObject<NitaResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

                    // map NITA response to NIRA response for the database record
                    var errorVerificationResponse = new ChangePasswordResponse
                    {
                        IsError = errorResponse.Return.TransactionStatus.TransactionStatusTransactionStatus.ToLower() == "error",
                        PasswordDaysLeft = errorResponse.Return.TransactionStatus.PasswordDaysLeft.ToString(),
                        Status = errorResponse.Return.TransactionStatus.TransactionStatusTransactionStatus,
                        Error = new ResponseError
                        {
                            Code = errorResponse.Return.TransactionStatus.Error.Code.ToString(),
                            Message = errorResponse.Return.TransactionStatus.Error.Message
                        }
                    };

                    _logger.LogInformation(
                        "Failed to change password for {NiraUsername} using the NITA connection with error code: {VerificationResponseErrorCode} because {VerificationResponseErrorMessage}", username, errorVerificationResponse.Error.Code, errorVerificationResponse.Error.Message);

                    return errorVerificationResponse;

                    break;

                default:
                    _logger.LogWarning(
                        "Failed to change password for {NiraUsername} through NITA statusCode:{ResponseStatusCode} Message:{ResponseRequestMessage}", username, response.StatusCode, response.RequestMessage);

                    throw new NotFoundException($"Failed to change password for {username}.");

                    break;
            }

        }

        private void PrepareNitaHeaders(string username, string password, HttpRequestMessage request)
        {
            var userNameToken = NiraUtils.SetUsernameToken(username, password, _nitaOptions.Value.Offset, _nitaOptions.Value.Culture);

            // create nita-auth-forward
            var nitaConcat = $"{username}:{userNameToken.Password}".GetByteArrayFromString();
            var nitaAuthForward = NiraUtils.ConvertToBase64(nitaConcat);

            request.Headers.Add("nira-auth-forward", nitaAuthForward);
            request.Headers.Add("nira-nonce", userNameToken.Nonce);
            request.Headers.Add("nira-created", userNameToken.Created);
        }
    }
}
