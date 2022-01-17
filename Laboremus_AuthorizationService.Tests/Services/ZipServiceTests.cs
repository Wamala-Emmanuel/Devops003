using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Laboremus_AuthorizationService.Core.Extensions;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Models;
using Laboremus_AuthorizationService.Repositories.ExportRequests;
using Laboremus_AuthorizationService.Services.DirectoryService;
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
    public class ZipServiceTests
    {
        private readonly Mock<IDirectoryService> _mockDirectoryService;
        private readonly Mock<IExportRequestRepository> _mockExportRequestRepository;
        private readonly MockFileSystem _mockFileSystem;
        private readonly ILogger<ZipService> _logger;
        private readonly ZipService _zipService;
        private readonly Guid _requestId;
        private readonly ExportRequest _savedRequest;
        private readonly ExportRequest _updatedRequest;
        private readonly List<ExportRequest> _requestList;
        private readonly UserExportRequest _request;
        private readonly ExportSettings _exportSettings = TestsHelper.GetExportSettings();

        public ZipServiceTests(ITestOutputHelper testOutputHelper)
        {
            _request = TestsHelper.GetUserExportRequest();

            _requestList = TestsHelper.GetListOfExportRequests();

            _savedRequest = TestsHelper.GetExportRequest();

            _requestId = _savedRequest.Id;

            _savedRequest.Request = JsonConvert.SerializeObject(_request);

            _updatedRequest = _savedRequest;

            _updatedRequest.GenerationStatus = ExportStatus.Complete;

            _logger = testOutputHelper.BuildLoggerFor<ZipService>();

            var exportOptionsMock = new Mock<IOptions<ExportSettings>>();

            exportOptionsMock.Setup(x => x.Value)
                .Returns(_exportSettings);

            _mockFileSystem = new MockFileSystem();

            _mockFileSystem.AddFile($"./folder/{_requestId}.csv", new MockFileData("Id,Email,Lockout"));

            _mockFileSystem.AddFile($"./folder/{_requestId}.zip", new MockFileData("Id,Email,Lockout"));

            _mockExportRequestRepository = new Mock<IExportRequestRepository>();

            _mockExportRequestRepository
                .Setup(mr => mr.FirstAsync(It.IsAny<Expression<Func<ExportRequest, bool>>>()))
                .ReturnsAsync(_savedRequest);

            _mockExportRequestRepository
                .Setup(mr => mr.UpdateAsync(It.IsAny<Guid>(), It.IsAny<ExportRequest>()))
                .Returns(Task.CompletedTask);

            _mockExportRequestRepository.Setup(mr => mr.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockExportRequestRepository.Setup(mr =>
                mr.GetNotDownloadedRequestsExportListAsync(
                    It.IsAny<int>(), It.IsAny<double>()))
                .ReturnsAsync(_requestList);

            _mockDirectoryService = new Mock<IDirectoryService>();

            _mockDirectoryService.Setup(ms => ms.FileExists(It.IsAny<string>())).Returns(true);

            _mockDirectoryService.Setup(ms => ms.DeleteFile(It.IsAny<string>()));
        
            _zipService = new ZipService(_mockFileSystem, _mockExportRequestRepository.Object, _mockDirectoryService.Object,
                 _logger, exportOptionsMock.Object);
        }


        [Fact]
        public async Task ZipFileAsync_ShouldNotThrowException()
        {
            await _zipService.ZipFileAsync(_requestId);
        }

        [Fact]
        public async Task ZipFileAsync_ShouldFindRequest()
        {
            await _zipService.ZipFileAsync(_requestId);

            _mockExportRequestRepository.Verify(
                mr => mr.FirstAsync(
                    It.Is<Expression<Func<ExportRequest, bool>>>(r => r.Compile()(_savedRequest))), Times.Once);
        }

        [Fact]
        public async Task ZipFileAsync_ShouldThrowsExceptionWhenRequestIsNull()
        {
            _mockExportRequestRepository.Setup(
                mr => mr.FirstAsync(It.IsAny<Expression<Func<ExportRequest, bool>>>()))
                .ReturnsAsync((ExportRequest)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _zipService.ZipFileAsync(_requestId));
        }

        [Fact]
        public async Task ZipFileAsync_ShouldUpdateRequest()
        {
            await _zipService.ZipFileAsync(_requestId);

            _mockExportRequestRepository.Verify(
                mr => mr.UpdateAsync(
                    It.Is<object>( id  => id.Equals(_requestId)), 
                    It.Is<ExportRequest>(x =>
                        x.Id == _updatedRequest.Id &&
                        x.GenerationStatus == _updatedRequest.GenerationStatus &&
                        x.CreatedOn == _updatedRequest.CreatedOn &&
                        x.DownloadedOn == _updatedRequest.DownloadedOn &&
                        x.FileName == _updatedRequest.FileName &&
                        x.IsDeleted == _updatedRequest.IsDeleted)), Times.Once);
        }


        [Fact]
        public async Task Service_ShouldSaveExportsRequest()
        {
            await _zipService.ZipFileAsync(_requestId);

            _mockExportRequestRepository.Verify(
                mr => mr.SaveChangesAsync(), Times.Once);
        }


        [Fact]
        public async Task DeleteDownloadedZipFile_ShouldNotThrowException()
        {
            await _zipService.DeleteDownloadedZipFileAsync(_requestId);
        }

        [Fact]
        public async Task DeleteDownloadedZipFile_ShouldFindRequest()
        {
            await _zipService.DeleteDownloadedZipFileAsync(_requestId);

            _mockExportRequestRepository.Verify(
                mr => mr.FirstAsync(
                    It.Is<Expression<Func<ExportRequest, bool>>>(r => r.Compile()(_savedRequest))), Times.Once);
        }

        [Fact]
        public async Task DeleteDownloadedZipFile_ThrowsExceptionWhenRequestIsNull()
        {
            _mockExportRequestRepository.Setup(
                mr => mr.FirstAsync(It.IsAny<Expression<Func<ExportRequest, bool>>>()))
                .ReturnsAsync((ExportRequest)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _zipService.ZipFileAsync(_requestId));
        }

        [Fact]
        public async Task DeleteDownloadedZipFile_ThrowsExceptionWhenFileDoesnotExist()
        {
            _mockDirectoryService.Setup(ms => ms.FileExists(It.IsAny<string>())).Returns(false);

            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => _zipService.ZipFileAsync(_requestId));
        }

        [Fact]
        public async Task DeleteDownloadedZipFile_ShouldUpdateRequest()
        {
            await _zipService.DeleteDownloadedZipFileAsync(_requestId);

            _mockExportRequestRepository.Verify(
                mr => mr.UpdateAsync(
                    It.Is<object>(id => id.Equals(_requestId)),
                    It.Is<ExportRequest>(x =>
                        x.Id == _updatedRequest.Id &&
                        x.GenerationStatus == _updatedRequest.GenerationStatus &&
                        x.CreatedOn == _updatedRequest.CreatedOn &&
                        x.DownloadedOn == _updatedRequest.DownloadedOn &&
                        x.FileName == _updatedRequest.FileName &&
                        x.IsDeleted == _updatedRequest.IsDeleted)), Times.Once); ;
        }

        [Fact]
        public async Task DeleteRequestExport_ShouldNotThrowException()
        {
            await _zipService.DeleteRequestExportAsync();
        }

        [Fact]
        public async Task DeleteRequestExport_ShouldGetExportRequestList()
        {
            await _zipService.DeleteRequestExportAsync();

            _mockExportRequestRepository.Verify(
                mr => mr.GetNotDownloadedRequestsExportListAsync(
                    It.Is<int>(x => x == _exportSettings.DaysBack),
                    It.Is<double>(x => x == _exportSettings.Offset)), Times.Once);
        }

        [Fact]
        public async Task DeleteRequestExport_ShouldUpdateRequest()
        {
            await _zipService.DeleteRequestExportAsync();

            _mockExportRequestRepository.Verify(
                mr => mr.UpdateAsync(
                    It.Is<object>(id => id.Equals(_requestList[0].Id)),
                    It.Is<ExportRequest>(x =>
                        x.Id == _requestList[0].Id &&
                        x.GenerationStatus == _requestList[0].GenerationStatus &&
                        x.CreatedOn == _requestList[0].CreatedOn &&
                        x.DownloadedOn == _requestList[0].DownloadedOn &&
                        x.FileName == _requestList[0].FileName &&
                        x.IsDeleted == _requestList[0].IsDeleted))
                    , Times.Once);
        }

        [Fact]
        public async Task DeleteRequestExport_ShouldNotUpdateRequestWhenListisEmpty()
        {
            _mockExportRequestRepository.Setup(mr =>
                mr.GetNotDownloadedRequestsExportListAsync(
                    It.IsAny<int>(), It.IsAny<double>()))
                .ReturnsAsync((List<ExportRequest>)null);

            await _zipService.DeleteRequestExportAsync();

            _mockExportRequestRepository.Verify(
                mr => mr.UpdateAsync(
                    It.Is<object>(id => id.Equals(_requestList[0].Id)),
                    It.Is<ExportRequest>(x =>
                        x.Id == _requestList[0].Id &&
                        x.GenerationStatus == _requestList[0].GenerationStatus &&
                        x.CreatedOn == _requestList[0].CreatedOn &&
                        x.DownloadedOn == _requestList[0].DownloadedOn &&
                        x.FileName == _requestList[0].FileName &&
                        x.IsDeleted == _requestList[0].IsDeleted))
                    , Times.Never);
        }
    }
}
