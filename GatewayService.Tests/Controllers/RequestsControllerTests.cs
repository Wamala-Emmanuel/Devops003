using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GatewayService.Controllers;
using GatewayService.DTOs;
using GatewayService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace GatewayService.Tests.Controllers
{
    public class RequestsControllerTests : BaseControllerTests
    {
        private readonly Mock<HttpRequest> _mockRequest;
        private readonly Mock<IVerificationRequestService> _verificationServiceMock;
        private readonly Mock<IRequestService> _requestServiceMock;
        private readonly ILogger<VerificationRequestService> _logger;
        private readonly Guid _requestId = Guid.NewGuid();
        private readonly SearchRequest _searchRequest = TestHelper.GetTestSearchRequest();
        private readonly SearchResponse _response;
        private readonly NationalIdVerificationRequest _verificationRequest = TestHelper.GetTestVerificationRequest();
        private readonly RequestsController _requestController;

        public RequestsControllerTests(ITestOutputHelper testOutputHelper)
        {
            _requestServiceMock = new Mock<IRequestService>();

            _verificationServiceMock = new Mock<IVerificationRequestService>();

            _logger = testOutputHelper.BuildLoggerFor<VerificationRequestService>();

            _mockRequest = TestHelper.CreateMockRequest(_searchRequest);
            
            _response = TestHelper.GetTestSearchResponseResults(_searchRequest);

            _requestController = new RequestsController(_verificationServiceMock.Object, _requestServiceMock.Object, _logger)
            {
                ControllerContext = controllerContext
            };

            _verificationServiceMock.Setup(x => x.GetRequestsAsync(It.IsAny<SearchRequest>()));

            _requestServiceMock.Setup(x => x.Process(It.IsAny<NationalIdVerificationRequest>(), _mockRequest.Object));
        }

        [Fact]
        public async Task SearchRequestsAsync_ShouldNotThrowExceptionAsync()
        {
            await _requestController.SearchRequestsAsync(_searchRequest);
        }

        [Fact]
        public async Task SearchRequestsAsync_ShouldCallService()
        {
            await _requestController.SearchRequestsAsync(_searchRequest);

            _verificationServiceMock.Verify(x => x.GetRequestsAsync(It.Is<SearchRequest>(y => y.CardNumber == _searchRequest.CardNumber)),
                Times.Once);
        }

        [Fact]
        public async Task SearchRequestsAsync_ShouldReturnResponseAndValidStatus()
        {
            var response = await _requestController.SearchRequestsAsync(_searchRequest);

            Assert.NotNull(response);
            Assert.IsType<OkObjectResult>(response);
        }

        [Fact]
        public async Task GetRequestStatus_ShouldNotThrowExceptionAsync()
        {
            _verificationServiceMock.Setup(x => x.GetRequestsAsync(It.IsAny<SearchRequest>())).ReturnsAsync(_response);

            await _requestController.GetRequestStatus(_requestId);
        }
        
        [Fact]
        public async Task GetRequestStatus_ShouldReturnResponseAndValidStatus()
        {
            _verificationServiceMock.Setup(x =>
            x.GetRequestStatusAsync(It.IsAny<Guid>())).ReturnsAsync(_response.Requests[0]);

            var response = await _requestController.GetRequestStatus(_requestId);

            Assert.NotNull(response);

            Assert.IsType<OkObjectResult>(response);
        }
        
        [Fact]
        public async Task GetRequestStatus_ShouldReturnNotFoundResponseWhenRequestDoesntExist()
        {
            _verificationServiceMock.Setup(
                x => x.GetRequestsAsync(It.IsAny<SearchRequest>()))
                .ReturnsAsync(new SearchResponse() { 
                    Requests = new List<RequestViewModel>()
                });

            var response = await _requestController.GetRequestStatus(_requestId);

            Assert.IsType<NotFoundResult>(response);
        }


        [Fact]
        public async Task GetRequestStatus_ShouldReturnBadRequestResponseWhenGivenAnEmptyGuid()
        {
            _verificationServiceMock.Setup(
                    x => x.GetRequestStatusAsync(It.IsAny<Guid>()))
                .ReturnsAsync((RequestViewModel?)null);

            var response = await _requestController.GetRequestStatus(Guid.Empty);

            Assert.IsType<BadRequestObjectResult>(response);
        }


        [Fact]
        public async Task NationalId_ShouldNotThrowExceptionAsync()
        {
            await _requestController.Create(_verificationRequest);
        }

        [Fact]
        public async Task NationalId_ShouldCallService()
        {
            await _requestController.Create(_verificationRequest);

            _requestServiceMock.Verify(x => x.Process(
                It.Is<NationalIdVerificationRequest>(y => y.CardNumber == _verificationRequest.CardNumber),
                    It.IsAny<HttpRequest>()),
                Times.Once);
        }

        [Fact]
        public async Task NationalId_ShouldReturnResponseAndValidStatusAsync()
        {
            var response = await _requestController.Create(_verificationRequest);

            Assert.NotNull(response);
            Assert.IsType<AcceptedAtActionResult>(response);
        }
    }
}