using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Laboremus_AuthorizationService.Services.DirectoryService
{
    public class DirectoryService : IDirectoryService
    {
        private readonly ILogger<DirectoryService> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly string _csvExtenstion = ".csv";

        public DirectoryService(ILogger<DirectoryService> logger) : this(logger, new FileSystem())
        {
            _logger = logger;
        }

        public DirectoryService(ILogger<DirectoryService> logger, IFileSystem fileSystem)
        {
            _logger = logger;
            _fileSystem = fileSystem;
        }

        public void CreateTempFile(string folderPath, string fileName, int buffer)
        {
            var fileFolder = fileName.Replace(_csvExtenstion, string.Empty);

            if (!_fileSystem.Directory.Exists(folderPath))
            {
                _fileSystem.Directory.CreateDirectory(folderPath);
            }

            CreateTempDirectory(folderPath, fileFolder);

            var filePath = _fileSystem.Path.Combine(folderPath, fileFolder, fileName);
            using (Stream stream = _fileSystem.File.Create(filePath, buffer))
            { };
        }

        public bool FileExists(string fileName)
        {
            return _fileSystem.File.Exists(fileName);
        }

        public void DeleteDirectory(string folderName)
        {
            try
            {
                _logger.LogInformation("Deleting folder: {0}", folderName);
                _fileSystem.Directory.Delete(folderName, true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error in deleting folder: {0}", folderName);
            }
        }

        public void DeleteFile(string fileName)
        {
            _fileSystem.File.Delete(fileName);
        }

        private void CreateTempDirectory(string folderPath, string fileName)
        {
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(folderPath, fileName));
        }
    }
}
