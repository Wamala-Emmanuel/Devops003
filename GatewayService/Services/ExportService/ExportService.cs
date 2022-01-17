using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using CsvHelper;
using GatewayService.DTOs;
using GatewayService.Helpers;
using GatewayService.Models;
using GatewayService.Repositories.Contracts;
using Hangfire;
using Hangfire.States;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GatewayService.Services
{
    public class ExportService : IExportService
    {
        private readonly IBackgroundJobClient _backgroundJob;
        private readonly IRequestCsvService _csvService;
        private readonly IDirectoryService _directoryService;
        private readonly IZipService _zipService;
        private readonly ILogger<ExportService> _logger;
        private readonly IRequestsExportRepository _exportRepository;
        private readonly ITokenUtil _tokenUtil;
        private readonly LinkGenerator _linkGenerator;
        private readonly double _offset;
        private readonly ExportSettings _exportSettings;
        private readonly string _nameClaim = "name";
        private readonly string _roleClaim = "role";

        public ExportService(IRequestCsvService csvService, IDirectoryService directoryService,
            IZipService zipService, IRequestsExportRepository exportRepository,
            IBackgroundJobClient backgroundJob, IConfiguration configuration,
            ILogger<ExportService> logger, ITokenUtil tokenUtil,
            IOptions<ExportSettings> exportOptions, LinkGenerator linkGenerator)
        {
            var niraConfig = configuration.GetNiraSettings();

            _backgroundJob = backgroundJob;
            _offset = niraConfig.NiraDateTimeConfig.Offset;
            _csvService = csvService;
            _directoryService = directoryService;
            _logger = logger;
            _exportRepository = exportRepository;
            _tokenUtil = tokenUtil;
            _linkGenerator = linkGenerator;
            _zipService = zipService;

            _exportSettings = exportOptions.Value;
        }

        public async Task<ExportStatusResponse> CheckRequestStatusAsync(Guid id, ApiVersion apiVersion)
        {
            var exportRequest = await _exportRepository.FindAsync(id);

            if (exportRequest == null)
            {
                throw new NotFoundException($"Failed to find export request with Id {id}.");
            }

            _logger.LogInformation("Retrieved export request with request Id: {RequestId}", id);

            return PrepareStatusResponse(exportRequest);
        }

        public async Task<FileViewModel> DownloadRequestsExportAsync(Guid id)
        {
            var exportRequest = await _exportRepository.FindAsync(id);

            if (exportRequest == null)
            {
                _logger.LogInformation("Failed to find export request with request Id: {ExportRequestId}.", id);
                throw new NotFoundException($"Failed to find export request with Id {id}.");
            }

            if (exportRequest.GenerationStatus != ExportStatus.Complete)
            {
                throw new ClientFriendlyException($"The export request with Id {id} can not be downloaded.");
            }

            _logger.LogInformation("Retrieved export request with request Id {ExportRequestId}", id);

            if (!_directoryService.FileExists(exportRequest.FileName))
            {
                _logger.LogError("Failed to find export file with Id {ExportRequestId}.", id);
                throw new NotFoundException($"Failed to find export file with Id {id}.");
            }

            var dataStream  = await _zipService.GetZipFileBytesAsync(exportRequest.FileName);

            _logger.LogInformation("Retrieved export request file contents with request Id: {ExportRequestId}.", exportRequest.Id);

            exportRequest.DownloadedOn = DateTime.UtcNow.AddHours(_offset);

            await _exportRepository.UpdateAsync(exportRequest);
            
            _logger.LogInformation("Export Request with request Id: {ExportRequestId} succesfully updated.", exportRequest.Id);

            return new FileViewModel
            {
                Name = $"{exportRequest.Id}.zip",
                Contents = dataStream,
                ContentType = MediaTypeNames.Application.Octet,
            };
        }

        public async Task<ExportStatusResponse> ExportAsync(ExportRequest request, HttpRequest httpRequest)
        {
            var userInfo = _tokenUtil.GetTokenClaims(httpRequest);

            if (_tokenUtil.ClaimExists(userInfo, _roleClaim))
            {
                userInfo = await _tokenUtil.GetUserInfo(httpRequest);
            }

            var userName = _tokenUtil.TryGetClaimValue(userInfo, _nameClaim);

            var requestId = Guid.NewGuid();
            
            var requestToAdd = new RequestsExport
            {
                Id = requestId,
                GenerationStatus = ExportStatus.Processing,
                CreatedOn = DateTime.UtcNow.AddHours(_offset),
                UserName = userName ?? null,
                Request = JsonConvert.SerializeObject(request)
            };

            requestToAdd.FileName = $"{requestToAdd.CreatedOn:MM-dd-yyyy}-{requestToAdd.CreatedOn:HH_mm_ss}-{_exportSettings.NamePartial}.csv";

            var newRequest = await _exportRepository.AddAsync(requestToAdd);

            _logger.LogInformation("Added export request with request Id: {ExportRequestId}.", newRequest.Id);

            _directoryService.CreateTempFile(_exportSettings.FolderPath, newRequest.FileName, $"{requestId}", _exportSettings.Buffer);

            _logger.LogInformation("File with the name '{FileName}' created successfully.", newRequest.FileName);

            var csvId = _backgroundJob.Enqueue(() => _csvService.WriteToCsvFileAsync(newRequest.Id));

            var zipId = _backgroundJob.ContinueJobWith(csvId, () => _zipService.ZipFileAsync(newRequest.Id));

            _backgroundJob.ContinueJobWith(zipId, () =>
                            ScheduleDeleteJob(newRequest.Id));

            return PrepareStatusResponse(newRequest);
        }

        public void ScheduleDeleteJob(Guid exportRequestId)
        {
            _backgroundJob.Schedule(() => _zipService.DeleteDownloadedZipFileAsync(exportRequestId),
                TimeSpan.FromHours(_exportSettings.DelayInHours));
        }

        private ExportStatusResponse PrepareStatusResponse(RequestsExport export)
        {
            var result = new ExportStatusResponse
            {
                Id = export.Id,
                Status = export.GenerationStatus
            };

            switch (export.GenerationStatus)
            {
                case ExportStatus.Complete:
                    result.RequestUri = _linkGenerator.GetPathByAction("download", "exports", values: new { id = export.Id });
                    break;
                case ExportStatus.Processing:
                case ExportStatus.Failed:
                    result.RequestUri = _linkGenerator.GetPathByAction("GetRequestStatus", "exports", values: new { id = export.Id });
                    break;
            }
            return result;
        }
    }
}
