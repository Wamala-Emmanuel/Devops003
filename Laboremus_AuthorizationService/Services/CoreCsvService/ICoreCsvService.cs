using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Laboremus_AuthorizationService.Services.CoreCsvService
{
    public interface ICoreCsvService
    {
        Task WriteRecordsToCsvFileAsync<T>(string fullPath, int page, List<T> records);
    }
}
