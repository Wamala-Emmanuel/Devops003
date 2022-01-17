using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Laboremus_AuthorizationService.Core.Extensions;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Models;
using Laboremus_AuthorizationService.Repositories.ExportRequests;
using Laboremus_AuthorizationService.Services.DirectoryService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Laboremus_AuthorizationService.Services.ZipService
{
    public class ZipService : IZipService
    {
        private readonly IDirectoryService _directoryService;
        private readonly IFileSystem _fileSystem;
        private readonly IExportRequestRepository _exportRequestRepository;
        private readonly ILogger<ZipService> _logger;
        private readonly ExportSettings _exportSettings;

        public ZipService(IExportRequestRepository exportRepository, IDirectoryService directoryService,
            ILogger<ZipService> logger, IOptions<ExportSettings> exportOptions)
            : this(new FileSystem(), exportRepository, directoryService, logger, exportOptions)
        {
            _logger = logger;
            _directoryService = directoryService;
            _exportRequestRepository = exportRepository;
            _exportSettings = exportOptions.Value;

        }

        public ZipService(IFileSystem fileSystem, IExportRequestRepository exportRepository, IDirectoryService directoryService,
            ILogger<ZipService> logger, IOptions<ExportSettings> exportOptions)
        {
            
            _logger = logger;
            _directoryService = directoryService;
            _exportRequestRepository = exportRepository;
            _fileSystem = fileSystem;
            _exportSettings = exportOptions.Value;
        }

        public async Task DeleteDownloadedZipFileAsync(Guid requestId)
        {
            var exportRequest = await _exportRequestRepository.FirstAsync(r => r.Id == requestId);

            if (exportRequest == null)
            {
                _logger.LogError("Failed to find export request with Id", requestId);
                throw new NotFoundException($"Failed to find export request with Id {requestId}");
            }

            _logger.LogInformation("Retrieved export request with request Id {0}", requestId);

            var filePath = exportRequest.FileName;

            if (!_directoryService.FileExists(filePath))
            {
                throw new NotFoundException($"Failed to find export file with the name {exportRequest.FileName}");
            }

            _directoryService.DeleteFile(filePath);

            exportRequest.IsDeleted = true;

            await _exportRequestRepository.UpdateAsync(exportRequest.Id, exportRequest);
            
            await _exportRequestRepository.SaveChangesAsync();

            _logger.LogInformation("Export File {0} has been successfully deleted.", exportRequest.FileName);
        }

        public async Task DeleteRequestExportAsync()
        {
            var exportRequestsList = await _exportRequestRepository.GetNotDownloadedRequestsExportListAsync(
                _exportSettings.DaysBack, _exportSettings.Offset);

            if (exportRequestsList != null)
            {
                _logger.LogInformation("Retrieved export request files that arenot downloaded");

                foreach (var request in exportRequestsList)
                {
                    var filePath = request.FileName;

                    if (_directoryService.FileExists(filePath))
                    {
                        _directoryService.DeleteFile(filePath);
                    }

                    request.IsDeleted = true;


                    await _exportRequestRepository.UpdateAsync(request.Id, request);

                    await _exportRequestRepository.SaveChangesAsync();

                    _logger.LogInformation("Export File {0} has been successfully deleted.", request.FileName);
                }

            }
            else
            {
                _logger.LogInformation("All export requests have been downloaded.");
            }
        }

        public async Task<byte[]> GetZipFileBytesAsync(string filePath)
        {
            if (!_directoryService.FileExists(filePath))
            {
                _logger.LogError("Failed to find export file with the name {0}", filePath);
                throw new NotFoundException($"Failed to find export file with the name {filePath}");
            }

            using (var sourceStream = new FileStream(
                                        filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                                        bufferSize: _exportSettings.Buffer, useAsync: true))
            {
                using (var ms = new MemoryStream())
                {
                    byte[] buffer = new byte[0x1000];
                    int numRead;
                    while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        await ms.WriteAsync(buffer, 0, numRead);
                    }

                    _logger.LogInformation("Retrieved export request file bytes");

                    return ms.ToArray();
                }
            }
        }

        public async Task ZipFileAsync(Guid requestId)
        {
            var exportRequest = await _exportRequestRepository.FirstAsync(r => r.Id == requestId);

            if (exportRequest == null)
            {
                _logger.LogError("Failed to find export request with Id", requestId);
                throw new NotFoundException($"Failed to find export request with Id {requestId}");
            }

            _logger.LogInformation("Retrieved export request with request Id {0}", requestId);

            var exportPath = _fileSystem.Path.Combine(_exportSettings.FolderPath, $"{exportRequest.Id}");

            var zipPath = _fileSystem.Path.Combine(_exportSettings.FolderPath, $"{exportRequest.Id}.zip");

            if (!_directoryService.FileExists(zipPath))
            {
                ZipFile.CreateFromDirectory(exportPath, zipPath);

                _logger.LogInformation("Export request zip file with the name {0} created.", zipPath);
            }

            _directoryService.DeleteDirectory(exportPath);

            exportRequest.GenerationStatus = ExportStatus.Complete;

            exportRequest.FileName = zipPath;

            await _exportRequestRepository.UpdateAsync(exportRequest.Id, exportRequest);

            await _exportRequestRepository.SaveChangesAsync();

            _logger.LogInformation("Export Request with request Id {0} successfully updated.", requestId);

        }

    }
}
