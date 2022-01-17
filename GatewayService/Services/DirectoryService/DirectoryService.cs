using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GatewayService.Services
{
    public class DirectoryService : IDirectoryService
    {
        private readonly ILogger<DirectoryService> _logger;
        private readonly IFileSystem _fileSystem;

        public DirectoryService(ILogger<DirectoryService> logger) : this(logger, new FileSystem())
        {
            _logger = logger;
        }

        public DirectoryService(ILogger<DirectoryService> logger, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public void CreateTempFile(string folderPath, string fileName, string requestId, int buffer)
        {
            if (!_fileSystem.Directory.Exists(folderPath))
            {
                _fileSystem.Directory.CreateDirectory(folderPath);
            }

            CreateTempDirectory(folderPath, requestId);

            var filePath = _fileSystem.Path.Combine(folderPath, requestId, fileName);

            using Stream stream = _fileSystem.File.Create(filePath, buffer);
        }

        public bool FileExists(string fileName)
        {
            return _fileSystem.File.Exists(fileName);
        }
        
        public void DeleteDirectory(string folderName)
        {
            try
            {
                _logger.LogInformation("Deleting folder: {FolderName}", folderName);
                _fileSystem.Directory.Delete(folderName, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in deleting folder: {FolderName}", folderName);
            }
        }

        public void DeleteFile(string fileName)
        {
            _fileSystem.File.Delete(fileName);
        }

        public void RenameFile(string oldPath, string newPath)
        {
            try
            {
                var fileInfo = _fileSystem.FileInfo.FromFileName(oldPath);
                fileInfo.MoveTo(newPath);
                _logger.LogInformation("File {OldPath} renamed to {NewPath}.", oldPath, newPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in renaming file located at {OldPath}", oldPath);
            }
        }

        private void CreateTempDirectory(string folderPath, string fileName)
        {
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(folderPath, fileName));
        }
    }
}
