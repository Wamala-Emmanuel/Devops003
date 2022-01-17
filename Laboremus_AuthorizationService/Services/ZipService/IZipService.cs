using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Laboremus_AuthorizationService.Services.ZipService
{
    public interface IZipService
    {
        Task DeleteDownloadedZipFileAsync(Guid requestId);

        Task DeleteRequestExportAsync();

        Task<byte[]> GetZipFileBytesAsync(string filePath);

        Task ZipFileAsync(Guid requestId);
    }
}
