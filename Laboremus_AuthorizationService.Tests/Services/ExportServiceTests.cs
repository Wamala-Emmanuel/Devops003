using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Laboremus_AuthorizationService.Core.Exceptions;
using Laboremus_AuthorizationService.Core.Extensions;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Models;
using Laboremus_AuthorizationService.Repositories.ExportRequests;
using Laboremus_AuthorizationService.Services;
using Laboremus_AuthorizationService.Services.DirectoryService;
using Laboremus_AuthorizationService.Services.ExportService;
using Laboremus_AuthorizationService.Services.RequestCsvService;
using Laboremus_AuthorizationService.Services.ZipService;
using Laboremus_AuthorizationService.Tests.Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Laboremus_AuthorizationService.Tests.Services
{
    public class ExportServiceTests
    {
        private readonly Mock<IBackgroundJobClient> _jobMock;
        private readonly Mock<IDirectoryService> _mockDirectoryService;
        private readonly Mock<IRequestCsvService> _mockRequestCsvService;
        private readonly Mock<IZipService> _mockZipService;
        private readonly ExportService _exportService;
        private readonly Mock<IExportRequestRepository> _mockExportRequestRepository;
        private readonly UserExportRequest _request;
        private readonly ExportRequest _savedRequest;
        private readonly ExportRequest _ongoingRequest;
        private readonly ILogger<ExportService> _logger;

        private readonly Guid _requestId;
        private readonly ExportSettings _exportSettings = TestsHelper.GetExportSettings();

        public ExportServiceTests(ITestOutputHelper testOutputHelper)
        {
            _request = TestsHelper.GetUserExportRequest();

            _savedRequest = TestsHelper.GetExportRequest();

            _ongoingRequest = TestsHelper.GetExportRequest();

            _ongoingRequest.GenerationStatus = ExportStatus.Processing;
            
            _requestId = _savedRequest.Id;

            var exportOptionsMock = new Mock<IOptions<ExportSettings>>();

            exportOptionsMock.Setup(x => x.Value)
                .Returns(_exportSettings);

            _mockExportRequestRepository = new Mock<IExportRequestRepository>();

            _mockExportRequestRepository
                .Setup(mr => mr.UpdateAsync(It.IsAny<Guid>(), It.IsAny<ExportRequest>()))
                .Returns(Task.CompletedTask);

            _mockExportRequestRepository.Setup(mr => mr.AddAsync(It.IsAny<ExportRequest>()))
                .Returns(Task.CompletedTask);
            
            _mockExportRequestRepository.Setup(mr => mr.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockExportRequestRepository
                .Setup(mr => mr.FirstAsync(
                    It.IsAny<Expression<Func<ExportRequest, bool>>>()))
                .ReturnsAsync(_savedRequest);

            _mockDirectoryService = new Mock<IDirectoryService>();

            _mockDirectoryService.Setup(ds => ds.CreateTempFile(
                _exportSettings.FolderPath, _savedRequest.FileName, _exportSettings.Buffer));

            _mockDirectoryService.Setup(ds => ds.FileExists(It.IsAny<string>())).Returns(true);

            _mockRequestCsvService = new Mock<IRequestCsvService>();

            _mockZipService = new Mock<IZipService>();

            _logger = testOutputHelper.BuildLoggerFor<ExportService>();

            _jobMock = new Mock<IBackgroundJobClient>();

            _jobMock.Setup(x => x.Create(
                It.Is<Job>(job => job.Method.Name == nameof(IRequestCsvService.WriteToCsvFileAsync)), 
                It.IsAny<EnqueuedState>()))
                .Returns("1");

            _jobMock.Setup(x => x.Create(
                It.Is<Job>(job => job.Method.Name == nameof(IZipService.ZipFileAsync)), 
                It.IsAny<AwaitingState>())).Returns("2");

            _exportService = new ExportService(_mockDirectoryService.Object, _mockRequestCsvService.Object,
                _mockZipService.Object, _mockExportRequestRepository.Object, 
                exportOptionsMock.Object, _logger, _jobMock.Object);
        }

        [Fact]
        public async Task ExportAsync_ShouldNotThrowException()
        {
            await _exportService.ExportAsync(_request);
        }

        [Fact]
        public async Task Service_ShouldAddExportsRequest()
        {
            var jsonRequest = JsonConvert.SerializeObject(_request);
            await _exportService.ExportAsync(_request);

            _mockExportRequestRepository.Verify(
                mr => mr.AddAsync(
                    It.Is<ExportRequest>(y =>
                        y.Request == jsonRequest
                        && y.GenerationStatus == ExportStatus.Processing)), Times.Once);
        }
        
        [Fact]
        public async Task Service_ShouldSaveExportsRequest()
        {
            var jsonRequest = JsonConvert.SerializeObject(_request);
            await _exportService.ExportAsync(_request);

            _mockExportRequestRepository.Verify(
                mr => mr.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Service_ShouldCreateExportsRequestFile()
        {
            await _exportService.ExportAsync(_request);

            _mockDirectoryService.Verify(
                ds => ds.CreateTempFile(It.IsAny<string>(),
                    It.Is<string>(y => y == _savedRequest.FileName),
                    It.Is<int>(y => y == _exportSettings.Buffer)), Times.Once);
        }


        [Fact]
        public async Task Service_ShouldCallCsvService()
        {
            await _exportService.ExportAsync(_request);

            _jobMock.Verify(x => x.Create(
                It.Is<Job>(y =>
                    y.Method.Name == nameof(IRequestCsvService.WriteToCsvFileAsync)),
                        It.IsAny<EnqueuedState>()), Times.Once);

        }

        [Fact]
        public async Task Service_ShouldCallZipServiceToZip()
        {
            await _exportService.ExportAsync(_request);

            _jobMock.Verify(x => x.Create(
                It.Is<Job>(y =>
                    y.Method.Name == nameof(IZipService.ZipFileAsync)),
                        It.IsAny<AwaitingState>()), Times.Once);
        }

        [Fact]
        public async Task Service_ShouldReturnExportStatusResponse()
        {
            var result = await _exportService.ExportAsync(_request);

            Assert.IsType<ExportStatusResponse>(result);
            Assert.Equal(_savedRequest.Id, result.Id);
        }

        [Fact]
        public async Task CheckRequestStatusAsync_ShouldNotThrowException()
        {
            await _exportService.CheckRequestStatusAsync(_requestId);
        }

        [Fact]
        public async Task CheckRequestStatusAsync_ShouldFindRequest()
        {
            await _exportService.CheckRequestStatusAsync(_requestId);

            _mockExportRequestRepository.Verify(
                mr => mr.FirstAsync(
                    It.Is<Expression<Func<ExportRequest, bool>>>(r => r.Compile()(_savedRequest))), Times.Once);
        }

        [Fact]
        public async Task CheckRequestStatusAsync_ShouldThrowsExceptionWhenRequestIsNull()
        {
            _mockExportRequestRepository.Setup(
                mr => mr.FirstAsync(It.IsAny<Expression<Func<ExportRequest, bool>>>()))
                .ReturnsAsync((ExportRequest)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _exportService.CheckRequestStatusAsync(_requestId));
        }

        [Fact]
        public async Task CheckRequestStatusAsync_ReturnsRequestsExportObject()
        {
            var result = await _exportService.CheckRequestStatusAsync(_requestId);

            Assert.IsType<ExportStatusResponse>(result);
            Assert.Equal(_requestId, result.Id);
            Assert.Equal(ExportStatus.Complete, result.Status);
        }

        [Fact]
        public async Task CheckRequestStasusAsync_ReturnsRequestWithCompleteStatus_WhenProcessIsComplete()
        {
            _mockExportRequestRepository.Setup(mr => mr.FirstAsync(
                It.IsAny<Expression<Func<ExportRequest, bool>>>()))
                    .ReturnsAsync(new ExportRequest
                    {
                        Id = _requestId,
                        GenerationStatus = ExportStatus.Complete,
                        FileName = "location of file"
                    });

            var result = await _exportService.CheckRequestStatusAsync(_requestId);

            Assert.Equal(_requestId, result.Id);
            Assert.Equal(ExportStatus.Complete, result.Status);
        }

        [Fact]
        public async Task DownloadRequestsExportAsync_ShouldNotThrowException()
        {
            await _exportService.DownloadRequestsExportAsync(_requestId);
        }

        [Fact]
        public async Task DownloadRequestsExportAsync_ShouldFindRequest()
        {
            await _exportService.DownloadRequestsExportAsync(_requestId);

            _mockExportRequestRepository.Verify(
                mr => mr.FirstAsync(
                    It.Is<Expression<Func<ExportRequest, bool>>>(r => r.Compile()(_savedRequest))), Times.Once);
        }

        [Fact]
        public async Task DownloadRequestsExportAsync_ShouldThrowsExceptionWhenRequestIsNull()
        {
            _mockExportRequestRepository.Setup(
                mr => mr.FirstAsync(It.IsAny<Expression<Func<ExportRequest, bool>>>()))
                .ReturnsAsync((ExportRequest)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _exportService.DownloadRequestsExportAsync(_requestId));
        }
        
        [Fact]
        public async Task DownloadRequestsExportAsync_ShouldThrowsExceptionWhenRequestIsBeingProcessed()
        {
            _mockExportRequestRepository.Setup(
                mr => mr.FirstAsync(It.IsAny<Expression<Func<ExportRequest, bool>>>()))
                .ReturnsAsync(_ongoingRequest);

            await Assert.ThrowsAsync<ClientFriendlyException>(() => _exportService.DownloadRequestsExportAsync(_requestId));
        }

        [Fact]
        public async Task DownloadRequestsExportAsync_ShouldFindFile()
        {
            await _exportService.DownloadRequestsExportAsync(_requestId);

            _mockDirectoryService.Verify(
                mr => mr.FileExists(It.Is<string>(x => x == _savedRequest.FileName)), Times.Once);
        }
        
        [Fact]
        public async Task DownloadRequestsExport_ShouldThrowsExceptionWhenFileDoesnotExist()
        {
            _mockDirectoryService.Setup(ds => ds.FileExists(It.IsAny<string>())).Returns(false);

            await Assert.ThrowsAsync<NotFoundException>(() => _exportService.DownloadRequestsExportAsync(_requestId));
        }

        [Fact]
        public async Task DownloadRequestsExport_ShouldGetFileContents()
        {
            await _exportService.DownloadRequestsExportAsync(_requestId);

            _mockZipService.Verify(
                zs => zs.GetZipFileBytesAsync(It.Is<string>(x => x == _savedRequest.FileName)), Times.Once);
        }

        [Fact]
        public async Task DownloadRequestsExport_ShouldUpdateRequestExport()
        {
            await _exportService.DownloadRequestsExportAsync(_requestId);

            _mockExportRequestRepository.Verify(
                mr => mr.UpdateAsync(
                    It.Is<object>(id => id.Equals(_requestId)),
                    It.Is<ExportRequest>(x =>
                        x.Id == _savedRequest.Id &&
                        x.GenerationStatus == _savedRequest.GenerationStatus &&
                        x.CreatedOn == _savedRequest.CreatedOn &&
                        x.DownloadedOn == _savedRequest.DownloadedOn &&
                        x.FileName == _savedRequest.FileName &&
                        x.IsDeleted == _savedRequest.IsDeleted)), Times.Once);
        }

        [Fact]
        public async Task DownloadRequestsExport_ReturnsRequestsExportObject()
        {
            var fileName = $"{_requestId}.zip";

            var result = await _exportService.DownloadRequestsExportAsync(_requestId);

            Assert.IsType<FileViewModel>(result);
            Assert.Equal(fileName, result.Name);
        }
    }
}
