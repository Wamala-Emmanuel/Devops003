using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using GatewayService.DTOs;
using GatewayService.Helpers;
using GatewayService.Helpers.Nira;
using GatewayService.Models;
using GatewayService.Repositories.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GatewayService.Services
{
    public class RequestCsvService : IRequestCsvService
    {
        private readonly ICoreCsvService _coreCsvService;
        private readonly IFileSystem _fileSystem;
        private readonly IRequestsExportRepository _exportRepository;
        private readonly IRequestRepository _requestRepository;
        private readonly ILogger<RequestCsvService> _logger;
        private readonly ExportSettings _exportSettings;
        private readonly NiraSettings _niraSettings;

        public RequestCsvService(ICoreCsvService coreCsvService, IRequestsExportRepository exportRepository, IRequestRepository requestRepository, 
                ILogger<RequestCsvService> logger, IOptions<ExportSettings> exportOptions, IOptions<NiraSettings> niraOptions)
            : this(new FileSystem(), coreCsvService, exportRepository, requestRepository, logger, exportOptions, niraOptions)
        {
            _coreCsvService = coreCsvService;
            _logger = logger;
            _exportRepository = exportRepository;
            _requestRepository = requestRepository;
            _exportSettings = exportOptions.Value;
            _niraSettings = niraOptions.Value;
        }

        public RequestCsvService(IFileSystem fileSystem, ICoreCsvService coreCsvService, IRequestsExportRepository exportRepository,
            IRequestRepository requestRepository, ILogger<RequestCsvService> logger,
            IOptions<ExportSettings> exportOptions, IOptions<NiraSettings> niraOptions)
        {
            _coreCsvService = coreCsvService;
            _fileSystem = fileSystem;
            _logger = logger;
            _exportRepository = exportRepository;
            _requestRepository = requestRepository;
            _exportSettings = exportOptions.Value;
            _niraSettings = niraOptions.Value;
        }

        public async Task WriteToCsvFileAsync(Guid requestId)
        {
            var exportRequest = await _exportRepository.FindAsync(requestId);

            if (exportRequest == null)
            {
                _logger.LogError("Failed to find export request with Id {RequestId}", requestId);
                throw new NotFoundException($"Failed to find export request with Id {requestId}");
            }

            _logger.LogInformation("Retrieved export request with request Id {RequestId}", requestId);

            var request = JsonConvert.DeserializeObject<ExportRequest>(exportRequest.Request ?? "{}",
                         new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var count = await _requestRepository.GetExportRequestCountAsync(request);
         
            var totalPages = Math.Ceiling((double)count / _exportSettings.PageSize);

            var fileFolder = $"{exportRequest.Id}";

            var fullPath = _fileSystem.Path.Combine(_exportSettings.FolderPath, fileFolder, exportRequest.FileName);

            if (count == 0)
            {
                await _coreCsvService.WriteRecordsToCsvFileAsync(fullPath, count, new List<ExportDto>());

                _logger.LogInformation("Found no requests for export request with id: {ExportRequestId}.", exportRequest.Id);
                return;
            }

            foreach (var page in Enumerable.Range(1, (int)totalPages))
            {
                var pagination = new ExportPagination
                {
                    ItemsPerPage = _exportSettings.PageSize,
                    Page = page,
                    TotalItems = count
                };

                var records = await _requestRepository.GetExportRequestListAsync(request, pagination);
                var recordsDto = GetExportResponse(records, _niraSettings);

                await _coreCsvService.WriteRecordsToCsvFileAsync(fullPath, page, recordsDto);
            }

            _logger.LogInformation("Successfully written requests to '{FileName}'.", exportRequest.FileName);

        }

        private static List<ExportDto> GetExportResponse(List<Request> items, NiraSettings niraSettings)
        {
            var requestList = new List<ExportDto>();

            foreach (var item in items)
            {
                requestList.Add(
                    new ExportDto
                    {
                        Reference = item.Id,
                        ReceivedAt = item.ReceivedAt,
                        Name = item.Name,
                        Nin = item.Nin,
                        CardNumber = item.CardNumber,
                        DateOfBirth = item.DateOfBirth.HasValue ? 
                                      item.DateOfBirth.Value.Date.ToString(
                                          niraSettings.NiraDateTimeConfig.ExportDateFormat, new CultureInfo(niraSettings.NiraDateTimeConfig.Culture)) : string.Empty,
                        User = item.InitiatorEmail ?? string.Empty,
                        RequestStatus = item.RequestStatus,
                        MatchStatus = GetMatchStatus(item.VerificationResult?.MatchingStatus),
                        CardStatus = GetCardStatus(item),
                        NinResponse = GetNinStatus(item.VerificationResult)
                    }
                );
            };

            return requestList;
        }

        private static string GetCardStatus(Request request)
        {
            return (request.VerificationResult?.CardStatus) switch
            {
                "Expired" => "Expired",
                "Valid" => "Valid",
                "Not valid" => "Invalid",
                "Not Active" => "Inactive",
                "Stop-listed" => "Blacklisted",
                null => request.VerificationResult?.IsError == true ? "Error" : "Pending",
                _ => string.Empty,
            };
            ;
        }

        private static string GetMatchStatus(bool? matchingStatus) => matchingStatus switch
        {
            true => "Match",
            false => "Mismatch",
            null => "Null",
        };

        private static string? GetNinStatus(VerificationResult? result)
        {
            return (result?.IsError) switch
            {
                true => result.Error.Code switch
                {
                    "320" => "Deceased",
                    "321" => "Not Citizen",
                    "322" => "Not Found",
                    _ => "Unknown Error",
                },
                false => "NIN Valid",
                _ => null,
            };
            ;
        }
    }
}
