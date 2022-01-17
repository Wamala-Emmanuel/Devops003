using Laboremus_AuthorizationService.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using Laboremus_AuthorizationService.Tests.Core.Helpers;
using Xunit;

namespace Laboremus_AuthorizationService.Tests.Core.Extensions
{
    public class ConfigExtensionsTests : IDisposable    
    {
        private readonly MockRepository _mockRepository;
        private readonly IConfiguration _config;

        public ConfigExtensionsTests()
        {
            this._mockRepository = new MockRepository(MockBehavior.Strict);
            _config = TestsHelper.GetIConfigurationRoot();

        }

        public void Dispose()
        {
            this._mockRepository.VerifyAll();
        }

        [Fact]
        public void GetAuthServer_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var config = this._config;

            // Act
            var result = config.GetAuthServer();

            // Assert
            Assert.True(!string.IsNullOrEmpty(result));
        }

        [Fact]
        public void GetAdDomain_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var config = this._config;
            
            // Act
            var result = config.GetAdDomain();

            // Assert
            Assert.True(!string.IsNullOrEmpty(result));
        }

        [Fact]
        public void OnPremiseAdEnabled_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var config = this._config;
            
            // Act
            var result = config.OnPremiseAdEnabled();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetSwaggerEnabled_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var config = this._config;
            
            // Act
            var result = config.GetSwaggerEnabled();

            // Assert
            Assert.True(result);
        }
    }
}
