using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using GatewayService.DTOs;
using GatewayService.Helpers;
using GatewayService.Models;
using GatewayService.Repositories.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GatewayService.Services
{
    public class ZipService : IZipService
    {
        private readonly IDirectoryService _directoryService;
        private readonly IFileSystem _fileSystem;
        private readonly IRequestsExportRepository _exportRepository;
        private readonly ILogger<ZipService> _logger;
        private readonly double _offset;
        private readonly ExportSettings _exportSettings;

        public ZipService(IRequestsExportRepository exportRepository, IDirectoryService directoryService,
            IConfiguration configuration, ILogger<ZipService> logger, IOptions<ExportSettings> exportOptions)
            : this(new FileSystem(), exportRepository, directoryService, configuration, logger, exportOptions)
        {
            var niraConfig = configuration.GetNiraSettings();

            _logger = logger;
            _directoryService = directoryService;
            _exportRepository = exportRepository;
            _offset = niraConfig.NiraDateTimeConfig.Offset;
            _exportSettings = exportOptions.Value;

        }

        public ZipService(IFileSystem fileSystem, IRequestsExportRepository exportRepository, 
            IDirectoryService directoryService, IConfiguration configuration, 
            ILogger<ZipService> logger, IOptions<ExportSettings> exportOptions)
        {
            var niraConfig = configuration.GetNiraSettings();
         
            _logger = logger;
            _directoryService = directoryService;
            _exportRepository = exportRepository;
            _fileSystem = fileSystem;
            _offset = niraConfig.NiraDateTimeConfig.Offset;
            _exportSettings = exportOptions.Value;
        }

        public async Task DeleteDownloadedZipFileAsync(Guid requestId)
        {
            var requestsExport = await _exportRepository.FindAsync(requestId);

            if (requestsExport == null)
            {
                _logger.LogError("Failed to find export request with Id {RequestId}", requestId);
                throw new NotFoundException($"Failed to find export request with Id {requestId}");
            }

            _logger.LogInformation("Retrieved export request with request Id: {RequestId}", requestId);

            var filePath = requestsExport.FileName;

            if (!_directoryService.FileExists(filePath))
            {
                _logger.LogError("Failed to find export file with the name {RequestsExportFileName}", requestsExport.FileName);
                throw new NotFoundException($"Failed to find export file with the name {requestsExport.FileName}");
            }

            _directoryService.DeleteFile(filePath);

            requestsExport.IsDeleted = true;

            await _exportRepository.UpdateAsync(requestsExport);

            _logger.LogInformation("Export File {FileName} has been successfully deleted.", requestsExport.FileName);
        }
        
        public async Task DeleteRequestExportAsync()
        {
            var requestsExportList = await _exportRepository.GetNotDownloadedRequestsExportListAsync(_exportSettings.DaysBack, _offset);

            if (requestsExportList != null)
            {
                _logger.LogInformation("Retrieved export request files that are not downloaded");

                foreach (var request in requestsExportList)
                {
                    var filePath = request.FileName;

                    if (_directoryService.FileExists(filePath))
                    {
                        _directoryService.DeleteFile(filePath);
                    }

                    request.IsDeleted = true;

                    await _exportRepository.UpdateAsync(request);

                    _logger.LogInformation("Export File {FileName} has been successfully deleted.", request.FileName);
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
                _logger.LogError("Failed to find export file with the name {FilePath}", filePath);
                throw new NotFoundException($"Failed to find export file with the name {filePath}");
            }

            using var sourceStream =
                new FileStream(
                    filePath,
                    FileMode.Open, FileAccess.Read, FileShare.Read,
                    bufferSize: _exportSettings.Buffer, useAsync: true);

            using var ms = new MemoryStream();

            byte[] buffer = new byte[0x1000];
            int numRead;
            while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                await ms.WriteAsync(buffer, 0, numRead);
            }

            _logger.LogInformation("Retrieved export request file bytes");

            return ms.ToArray();
        }

        public async Task ZipFileAsync(Guid requestId)
        {
            var requestsExport = await _exportRepository.FindAsync(requestId);

            if (requestsExport == null)
            {
                _logger.LogError("Failed to find export request with the id: {RequestId}", requestId);
                throw new NotFoundException($"Failed to find export request with Id {requestId}");
            }

            _logger.LogInformation("Retrieved export request with request Id: {RequestId}", requestId);

            var exportPath = _fileSystem.Path.Combine(_exportSettings.FolderPath, $"{requestsExport.Id}");

            var zipPath = _fileSystem.Path.Combine(_exportSettings.FolderPath, $"{requestsExport.Id}.zip");

            if (!_directoryService.FileExists(zipPath))
            {
                ZipFile.CreateFromDirectory(exportPath, zipPath);
                
                _logger.LogInformation("Export request zip file with the name {ZipPath} created.", zipPath);
            }

            _directoryService.DeleteDirectory(exportPath);

            requestsExport.GenerationStatus = ExportStatus.Complete;
                
            requestsExport.FileName = zipPath;

            await _exportRepository.UpdateAsync(requestsExport);

            _logger.LogInformation("Export Request with request Id: {RequestId} successfully updated.", requestId);

        }

    }
}
