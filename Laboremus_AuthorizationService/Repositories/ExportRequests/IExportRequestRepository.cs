using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laboremus_AuthorizationService.Models;

namespace Laboremus_AuthorizationService.Repositories.ExportRequests
{
    public interface IExportRequestRepository : IGenericRepository<ExportRequest>
    {
        /// <summary>
        /// Returns a list of Requestsexports that are completed but not downloaded after x days
        /// </summary>
        /// <param name="days"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        Task<List<ExportRequest>> GetNotDownloadedRequestsExportListAsync(int days, double offset);
    }
}
