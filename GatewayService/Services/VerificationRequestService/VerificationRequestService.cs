using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GatewayService.DTOs;
using GatewayService.Helpers;
using GatewayService.Helpers.Mappers;
using GatewayService.Helpers.Nira;
using GatewayService.Models;
using GatewayService.Repositories.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GatewayService.Services
{
    public class VerificationRequestService : IVerificationRequestService
    {
        private readonly IRequestRepository _repository;
        private readonly ILogger<VerificationRequestService> _logger;
        
        public VerificationRequestService(IRequestRepository repository, ILogger<VerificationRequestService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<SearchResponse> GetRequestsAsync(SearchRequest request)
        {
            var requestList = await _repository.GetAllPagedListAsync(request);
            
            return GetSearchResponse(requestList, request);
        }

        private static SearchResponse GetSearchResponse(List<Request> items, SearchRequest searchRequest)
        {
            var requestList = items.Select(MapperProfiles.MapRequestModelToRequestViewModel).ToList();

            var response = new SearchResponse
            {
                Pagination = new SearchPagination
                {
                    ItemsPerPage = searchRequest.Pagination.ItemsPerPage,
                    Page = searchRequest.Pagination.Page,
                    TotalItems = items.Count
                },

                Requests = requestList
            };
            return response;
        }

        public async Task<RequestViewModel?> GetRequestStatusAsync(Guid requestId)
        {
            var data = await _repository.FindAsync(requestId);
            if (data is null) return null;
            return MapperProfiles.MapRequestModelToRequestViewModel(data);
        }
    }
}
