using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Hangfire;
using Laboremus_AuthorizationService.Core.Exceptions;
using Laboremus_AuthorizationService.Core.Extensions;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Models;
using Laboremus_AuthorizationService.Repositories.ExportRequests;
using Laboremus_AuthorizationService.Services.DirectoryService;
using Laboremus_AuthorizationService.Services.RequestCsvService;
using Laboremus_AuthorizationService.Services.ZipService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Laboremus_AuthorizationService.Services.ExportService
{
    public class ExportService : IExportService
    {
        private readonly IExportRequestRepository _exportRequestRepository;
        private readonly IRequestCsvService _requestCsvService;
        private readonly IDirectoryService _directoryService;
        private readonly IZipService _zipService;
        private readonly ExportSettings _exportSettings;
        private readonly ILogger<ExportService> _logger;
        private readonly IBackgroundJobClient _backgroundJob;

        public ExportService(IDirectoryService directoryService, IRequestCsvService requestCsvService, 
            IZipService zipService, IExportRequestRepository exportRequestRepository, 
            IOptions<ExportSettings> exportOptions, ILogger<ExportService> logger, IBackgroundJobClient backgroundJob)
        {
            _directoryService = directoryService;
            _exportRequestRepository = exportRequestRepository;
            _exportSettings = exportOptions.Value;
            _logger = logger;
            _requestCsvService = requestCsvService;
            _zipService = zipService;
            _backgroundJob = backgroundJob;
        }

        public async Task<ExportStatusResponse> CheckRequestStatusAsync(Guid id)
        {
            var exportRequest = await _exportRequestRepository.FirstAsync(r => r.Id == id);

            if (exportRequest == null)
            {
                throw new NotFoundException($"Failed to find export request with Id {id}.");
            }

            _logger.LogInformation("Retrieved export request with request Id {0}.", id);

            return new ExportStatusResponse
            {
                Id = exportRequest.Id,
                Status = exportRequest.GenerationStatus
            };
        }

        public async Task<FileViewModel> DownloadRequestsExportAsync(Guid id)
        {
            var exportRequest = await _exportRequestRepository.FirstAsync(r => r.Id == id);

            if (exportRequest == null)
            {
                throw new NotFoundException($"Failed to find export request with Id {id}.");
            }

            if (exportRequest.GenerationStatus == ExportStatus.Processing)
            {
                throw new ClientFriendlyException($"The export request with Id {id} is still being processed.");
            }

            _logger.LogInformation("Retrieved export request with request Id {0}.", id);

            if (!_directoryService.FileExists(exportRequest.FileName))
            {
                throw new NotFoundException($"Failed to find export file with Id {id}.");
            }

            var dataStream = await _zipService.GetZipFileBytesAsync(exportRequest.FileName);

            _logger.LogInformation("Retrieved export request file contents with request Id {0}.", exportRequest.Id);

            exportRequest.DownloadedOn = DateTime.UtcNow.AddHours(_exportSettings.Offset);

            await _exportRequestRepository.UpdateAsync(exportRequest.Id, exportRequest);

            await _exportRequestRepository.SaveChangesAsync();

            _logger.LogInformation("Export Request with request Id {0} succesfully updated.", exportRequest.Id);

            return new FileViewModel
            {
                Name = $"{exportRequest.Id}.zip",
                Contents = dataStream,
                ContentType = MediaTypeNames.Application.Octet,
            };
        }

        public async Task<ExportStatusResponse> ExportAsync(UserExportRequest request)
        {
            var requestId = Guid.NewGuid();

            await _exportRequestRepository.AddAsync(new ExportRequest
            {
                Id = requestId,
                GenerationStatus = ExportStatus.Processing,
                CreatedOn = DateTime.UtcNow.AddHours(_exportSettings.Offset),
                FileName = $"{requestId}.csv",
                Request = JsonConvert.SerializeObject(request)
            });

            await _exportRequestRepository.SaveChangesAsync();

            var newRequest = await _exportRequestRepository.FirstAsync(r => r.Id == requestId);
            
            _logger.LogInformation("Added export request with request Id {0}.", newRequest.Id);

            _directoryService.CreateTempFile(_exportSettings.FolderPath, newRequest.FileName, _exportSettings.Buffer);

            _logger.LogInformation("File with the name '{0}' created successfully.", newRequest.FileName);

            var csvId = _backgroundJob.Enqueue(() => _requestCsvService.WriteToCsvFileAsync(newRequest.Id));

            var zipId = _backgroundJob.ContinueJobWith(csvId, () => _zipService.ZipFileAsync(newRequest.Id));

            _backgroundJob.ContinueJobWith(zipId, () =>
                            ScheduleDeleteJob(newRequest.Id));

            _logger.LogInformation("File with the name '{0}' zipped successfully.", newRequest.FileName);

            return new ExportStatusResponse
            {
                Id = newRequest.Id,
                Status = newRequest.GenerationStatus
            };
        }

        public void ScheduleDeleteJob(Guid exportRequestId)
        {
            _backgroundJob.Schedule(() => _zipService.DeleteDownloadedZipFileAsync(exportRequestId),
                TimeSpan.FromHours(_exportSettings.DelayInHours));
        }
    }
}
