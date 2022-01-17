using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Laboremus_AuthorizationService.Services.DirectoryService
{
    public interface IDirectoryService
    {
        void CreateTempFile(string folderPath, string fileName, int buffer);

        bool FileExists(string fileName);

        void DeleteDirectory(string folderName);

        void DeleteFile(string fileName);
    }
}
