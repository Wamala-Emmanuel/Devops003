using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayService.Context;
using GatewayService.DTOs;
using GatewayService.Helpers;
using GatewayService.Helpers.Nira;
using GatewayService.Models;
using GatewayService.Repositories.Contracts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GatewayService.Repositories.Implementation
{
    public class RequestRepository : IRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public RequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Return a list of verification requests
        /// </summary>
        /// <returns></returns>
        public async Task <List<Request>> GetAllPagedListAsync(SearchRequest request, CancellationToken cancellationToken = default)
        {
            var query = MakeSearchRequestQuery(request);

            var totalItems = await query.CountAsync(cancellationToken);

            query = query.OrderByDescending(r => r.ReceivedAt);

            return await query.Skip((request.Pagination.Page - 1) * request.Pagination.ItemsPerPage)
                .Take(request.Pagination.ItemsPerPage)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Requests.CountAsync(cancellationToken);
        }

        /// <summary>
        /// Checks whether a request with a given id exists
        /// </summary>
        /// <typeparam name="Request"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync<Request>(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Requests.AnyAsync(x => x.Id == id, cancellationToken);
        }

        /// <summary>
        /// Checks if a request with a given guid exists
        /// </summary>
        /// <param name="id"></param>    
        /// <returns></returns>
        public async Task<Request> FindAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Requests.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
        }


        /// <summary>
        /// Insert request in the database
        /// </summary>
        /// <param name="request"></param>    
        /// <returns>Request</returns>
        public async Task<Request> AddAsync(Request request, CancellationToken cancellationToken = default)
        {
            await _context.Requests.AddAsync(request, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return request;
        }

        /// <summary>
        /// Updates a request with a given guid
        /// </summary>
        /// <param name="request"></param>    
        /// <returns></returns>
        public async Task<Request> UpdateAsync(Request request, CancellationToken cancellationToken = default)
        {
            _context.Requests.Update(request);
            await _context.SaveChangesAsync(cancellationToken);
            return request;
        }

        public async Task<int> GetExportRequestCountAsync(ExportRequest request, CancellationToken cancellationToken = default)
        {
            return await MakeExportRequestQuery(request).CountAsync(cancellationToken);
        }

        public async Task<List<Request>> GetExportRequestListAsync(ExportRequest request, ExportPagination pagination, CancellationToken cancellationToken = default)
        {
            var query = MakeExportRequestQuery(request);

            var items = await query.Skip((pagination.Page - 1) * pagination.ItemsPerPage)
                            .Take(pagination.ItemsPerPage)
                            .ToListAsync(cancellationToken);
            return items;
        }

        private IQueryable<Request> MakeExportRequestQuery(ExportRequest request)
        {
            IQueryable<Request> query = _context.Requests;

            if (request.RequestStatus != null)
            {
                var enumList = request.RequestStatus
                                  .Select(x => (RequestStatus)Enum.Parse(typeof(RequestStatus), x))
                                  .ToList();

                return ComposeNinExportQuery(request, query, enumList);
            }
            else if (request.MatchingStatus != null)
            {
                var matchParam = new SqlParameter("match", request.MatchingStatus);

                var rawQuery = $@"
                    SELECT
                        *
                    FROM dbo.Requests
                    WHERE ISJSON(Result) > 0
                    AND JSON_VALUE(Result, '$.MatchingStatus') = @match
                ";

                if (request.NinValidity != null)
                {
                    var ninParam = new SqlParameter("validity", request.MatchingStatus);

                    var ninQuery = $@"
                        SELECT
                            *
                        FROM dbo.Requests
                        WHERE ISJSON(Result) > 0
                        AND JSON_VALUE(Result, '$.MatchingStatus') = @match
                        AND JSON_VALUE(Result, '$.cardStatus') = @validity
                    ";

                    query = _context.Requests.FromSqlRaw(rawQuery, matchParam, ninParam);
                }
                else
                {
                    query = _context.Requests.FromSqlRaw(rawQuery, matchParam);
                }
                return ComposeExportQuery(request, query);
            }
            else if (request.NinValidity != null)
            {
                var ninParam = new SqlParameter("match", request.NinValidity);

                var ninQuery = $@"
                    SELECT
                        *
                    FROM dbo.Requests
                    WHERE ISJSON(Result) > 0
                    AND JSON_VALUE(Result, '$.cardStatus') = @validity
                ";

                query = _context.Requests.FromSqlRaw(ninQuery, ninParam);
                return ComposeExportQuery(request, query);
            }
            else
            {
                return ComposeExportQuery(request, query);
            }

            query = query.OrderByDescending(r => r.ReceivedAt);

            return query;
        }

        private IQueryable<Request> ComposeNinExportQuery(
            ExportRequest request, IQueryable<Request> query, List<RequestStatus> requestStatuses)
        {
            if (request.NinValidity != null)
            {
                var ninParam = new SqlParameter("validity", request.NinValidity);

                if (request.MatchingStatus != null)
                {
                    var matchParam = new SqlParameter("match", request.MatchingStatus);

                    var rawQuery = $@"
                        SELECT
                            *
                        FROM dbo.Requests
                        WHERE ISJSON(Result) > 0
                        AND JSON_VALUE(Result, '$.cardStatus') = @validity
                        AND JSON_VALUE(Result, '$.MatchingStatus') = @match
                    ";

                    query = QueryNiraResult(requestStatuses, ninParam, matchParam, rawQuery);
                    return ComposeExportQuery(request, query);
                }
                else
                {
                    var ninQuery = $@"
                        SELECT
                            *
                        FROM dbo.Requests
                        WHERE ISJSON(Result) > 0
                        AND JSON_VALUE(Result, '$.cardStatus') = @validity
                    ";

                    query = QueryNiraResult(requestStatuses, ninParam, null, ninQuery);
                    return ComposeExportQuery(request, query);
                }
            }
            else
            {
                if (request.MatchingStatus != null)
                {
                    var matchParam = new SqlParameter("match", request.MatchingStatus);

                    var rawQuery = $@"
                        SELECT
                            *
                        FROM dbo.Requests
                        WHERE ISJSON(Result) > 0
                        AND JSON_VALUE(Result, '$.MatchingStatus') = @match
                    ";

                    query = QueryNiraResult(requestStatuses, null, matchParam, rawQuery);
                    return ComposeExportQuery(request, query);
                }
                query = query.Where(r => requestStatuses.Contains(r.RequestStatus));
                return ComposeExportQuery(request, query);
            }

            return query;
        }

        private IQueryable<Request> QueryNiraResult(List<RequestStatus> requestStatuses, SqlParameter? initialParam, SqlParameter? additionalParam, string rawQuery)
        {
            IQueryable<Request> query;
            if (requestStatuses.Count < 3)
            {
                query = _context.Requests.FromSqlRaw(rawQuery, initialParam, additionalParam)
                    .Where(r => requestStatuses.Contains(r.RequestStatus));
            }
            else
            {
                query = _context.Requests.FromSqlRaw(rawQuery, initialParam, additionalParam);
            }
            return query;
        }

        private static IQueryable<Request> ComposeExportQuery(ExportRequest request, IQueryable<Request> query)
        {
            query = query.Where(r => r.ReceivedAt >= request.DateRange.FromDate);

            query = query.Where(r => r.ReceivedAt <= request.DateRange.ToDate);

            if (!string.IsNullOrWhiteSpace(request.Nin))
            {
                query = query.Where(r => r.Nin.ToLower().Contains(request.Nin.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(request.CardNumber))
            {
                query = query.Where(r => r.CardNumber.ToLower().Contains(request.CardNumber.ToLower()));
            }

            return query;
        }

#nullable enable
        private IQueryable<Request> MakeSearchRequestQuery(SearchRequest request)
        {

            IQueryable<Request> query = _context.Requests;

            if (request.Status.HasValue && request.MatchingStatus.HasValue)
            {
                var matchParam = new SqlParameter("match", request.MatchingStatus.Value.ToString());

                var rawQuery = $@"
                    SELECT
                        *
                    FROM dbo.Requests
                    WHERE ISJSON(Result) > 0
                    AND JSON_VALUE(Result, '$.MatchingStatus') = @match
                ";

                query = _context.Requests.FromSqlRaw(rawQuery, matchParam)
                    .Where(r => r.RequestStatus == request.Status.Value);

                query = ComposeSearchQuery(request, query);
                return query;
            }

            if (request.Status.HasValue)
            {
                query = query.Where(r => r.RequestStatus == request.Status.Value);
            }

            if (request.MatchingStatus.HasValue)
            {
                var matchParam = new SqlParameter("match", request.MatchingStatus.Value.ToString());

                var rawQuery = $@"
                    SELECT
                        *
                    FROM dbo.Requests
                    WHERE ISJSON(Result) > 0
                    AND JSON_VALUE(Result, '$.MatchingStatus') = @match
                ";

                query = _context.Requests.FromSqlRaw(rawQuery, matchParam);
            }

            query = ComposeSearchQuery(request, query);

            return query;
        }

        private IQueryable<Request> ComposeSearchQuery(SearchRequest request, IQueryable<Request> query)
        {
            if (request.Id.HasValue)
            {
                query = query.Where(r => r.Id == request.Id.Value);
            }

            if (request.Date != null)
            {
                if (request.Date.From.HasValue)
                {
                    query = query.Where(r => r.ReceivedAt >= request.Date.From.Value);
                }

                if (request.Date.To.HasValue)
                {
                    query = query.Where(r => r.ReceivedAt <= request.Date.To.Value);
                }

            }

#nullable disable
            if (!string.IsNullOrWhiteSpace(request.Nin))
            {
                query = query.Where(r => r.Nin.ToLower().Contains(request.Nin.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(request.Surname))
            {
                query = query.Where(r => r.Surname.ToLower().Contains(request.Surname.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(request.GivenNames))
            {
                query = query.Where(r => r.GivenNames.ToLower().Contains(request.GivenNames.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(request.CardNumber))
            {
                query = query.Where(r => r.CardNumber.ToLower().Contains(request.CardNumber.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(request.Initiator))
            {
                query = query.Where(r => r.Initiator.ToLower().Contains(request.Initiator.ToLower()));
            }

            return query;
        }
    }
}
