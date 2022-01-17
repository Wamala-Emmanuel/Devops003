using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Laboremus_AuthorizationService.Core.Exceptions;
using Laboremus_AuthorizationService.Core.Extensions;
using Laboremus_AuthorizationService.Core.Helpers;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Models;
using Laboremus_AuthorizationService.Repositories.ExportRequests;
using Laboremus_AuthorizationService.Services.CoreCsvService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Laboremus_AuthorizationService.Services.RequestCsvService
{
    public class RequestCsvService : IRequestCsvService
    {
        private readonly ICoreCsvService _coreCsvService;
        private readonly IFileSystem _fileSystem;
        private readonly IExportRequestRepository _exportRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RequestCsvService> _logger;
        private readonly ExportSettings _exportSettings;
        private readonly string _emailClaim = "email";
        private readonly string _nameClaim = "name";
        private readonly List<string> _superAdminList = new List<string>() {
            "administrator",
            "administrator@laboremus.no",
        };

        public RequestCsvService(ICoreCsvService coreCsvService, IExportRequestRepository exportRepository, 
            UserManager<ApplicationUser> userManager, ILogger<RequestCsvService> logger, IOptions<ExportSettings> exportOptions)
            : this(new FileSystem(), coreCsvService, exportRepository, userManager, logger, exportOptions)
        {
            _coreCsvService = coreCsvService;
            _logger = logger;
            _exportRepository = exportRepository;
            _userManager = userManager;
            _exportSettings = exportOptions.Value;
        }

        public RequestCsvService(IFileSystem fileSystem, ICoreCsvService coreCsvService, 
            IExportRequestRepository exportRepository, UserManager<ApplicationUser> userManager, 
            ILogger<RequestCsvService> logger, IOptions<ExportSettings> exportOptions)
        {
            _coreCsvService = coreCsvService;
            _fileSystem = fileSystem;
            _logger = logger;
            _exportRepository = exportRepository;
            _userManager = userManager;
            _exportSettings = exportOptions.Value;
        }

        public async Task WriteToCsvFileAsync(Guid requestId)
        {
            var exportRequest = await _exportRepository.FindAsync(requestId);

            if (exportRequest == null)
            {
                throw new NotFoundException($"Failed to find export request with Id {requestId}");
            }

            _logger.LogInformation("Retrieved export request with request Id {0}", requestId);

            var request = JsonConvert.DeserializeObject<UserExportRequest>(exportRequest.Request ?? "{}",
                         new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var count = _userManager.Users.Count();

            var totalPages = Math.Ceiling((double)count / _exportSettings.PageSize);

            var fileFolder = exportRequest.FileName.Replace(".csv", string.Empty);

            var fullPath = _fileSystem.Path.Combine(_exportSettings.FolderPath, fileFolder, exportRequest.FileName);

            foreach (var page in Enumerable.Range(1, (int)totalPages))
            {
                var pagination = new ExportPagination
                {
                    ItemsPerPage = _exportSettings.PageSize,
                    Page = page,
                    TotalItems = count
                };

                var records = await GetUsersListAsync(request, pagination);
                var recordsDto = await MapUsersToUserExportViewModelAsync(records, request);

                await _coreCsvService.WriteRecordsToCsvFileAsync(fullPath, page, recordsDto);
            }

            _logger.LogInformation("Succesfully written requests to '{0}'.", exportRequest.FileName);

        }

        private IQueryable<ApplicationUser> MakeUserExportRequestQuery(UserExportRequest request)
        {
            var query = _userManager.Users;

            if (request.lockedOut.HasValue)
            {
                query = query.Where(q => q.LockoutEnabled == request.lockedOut);
            }

            // remove super admins from query
            query = query.Where(q => !_superAdminList.Contains(q.UserName));
            return query;
        }

        private async Task<List<ApplicationUser>> GetUsersListAsync(UserExportRequest request, ExportPagination pagination)
        {
            IQueryable<ApplicationUser> query = MakeUserExportRequestQuery(request);

            if (await query.CountAsync() >= _exportSettings.RequestLimit)
            {
                throw new ClientFriendlyException(
                    $"The export request exceeds the maximum {_exportSettings.RequestLimit} for exporting as configured.");
            }

            // default Page = 2
            var skip = (pagination.Page - 1) * pagination.ItemsPerPage;

            var users = await query.Skip(skip).Take(pagination.ItemsPerPage).ToListAsync();

            return users;
        }

        private async Task<List<UserExportViewModel>> MapUsersToUserExportViewModelAsync(List<ApplicationUser> users, UserExportRequest request)
        {
            var usersList = new List<UserExportViewModel>();

            foreach (var user in users)
            {
                var userDetail = new UserExportViewModel
                {
                    Id = user.Id,
                    Status = user.LockoutEnabled
                };

                var userRoles = await _userManager.GetRolesAsync(user);

                if (request.Roles != null)
                {
                    foreach (var role in request.Roles)
                    {
                        if (userRoles.Contains(role))
                        {
                            await SetUserRoleAndClaimsAsync(usersList, user, userDetail, userRoles);
                        }
                    }
                }
                else
                {
                    await SetUserRoleAndClaimsAsync(usersList, user, userDetail, userRoles);
                }

            }

            return usersList;
        }

        private async Task SetUserRoleAndClaimsAsync(List<UserExportViewModel> usersList, ApplicationUser user, UserExportViewModel userDetail, IList<string> userRoles)
        {
            if (userRoles?.Count > 0)
            {
                userDetail.Role = StringExtensions.CapitalizeFirstLetter(userRoles[0].Replace("_", " "));
            }
            await GetUserClaimsValuesAsync(user, userDetail);

            usersList.Add(userDetail);
        }

        private async Task GetUserClaimsValuesAsync(ApplicationUser user, UserExportViewModel userDetail)
        {
            var claims = await _userManager.GetClaimsAsync(user);

            if (claims?.Count > 0)
            {
                foreach (var claim in claims)
                {
                    if (!userDetail.Claims.Contains(claim))
                    {
                        userDetail.Claims.Add(claim);
                    }
                }

                userDetail.Email = StringExtensions.TryGetClaimValue(userDetail.Claims, _emailClaim);
                userDetail.Fullname = StringExtensions.TryGetClaimValue(userDetail.Claims, _nameClaim);
            }
        }
    }
}
