using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laboremus_AuthorizationService.Data;
using Laboremus_AuthorizationService.Models;
using Microsoft.EntityFrameworkCore;

namespace Laboremus_AuthorizationService.Repositories.ExportRequests
{
    public class ExportRequestRepository : GenericRepository<ExportRequest>, IExportRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public ExportRequestRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<ExportRequest>> GetNotDownloadedRequestsExportListAsync(int days, double offset)
        {
            var backDate = DateTime.UtcNow.AddDays(-days).AddHours(offset);

            IQueryable<ExportRequest> query = _context.ExportRequests;

            query = query.Where(r => r.GenerationStatus == ExportStatus.Complete);

            query = query.Where(r => r.DownloadedOn == null);

            query = query.Where(r => r.CreatedOn <= backDate);

            return query.ToListAsync();
        }
    }
}
