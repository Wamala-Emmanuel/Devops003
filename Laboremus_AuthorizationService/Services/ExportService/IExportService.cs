using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Models;
using Microsoft.AspNetCore.Mvc;

namespace Laboremus_AuthorizationService.Services.ExportService
{
    public interface IExportService
    {
        Task<ExportStatusResponse> CheckRequestStatusAsync(Guid id);

        Task<FileViewModel> DownloadRequestsExportAsync(Guid id);

        Task<ExportStatusResponse> ExportAsync(UserExportRequest request);
    }
}
