using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace Laboremus_AuthorizationService.Tests.Core.Helpers
{
    public class TestFixture<TStartup> : IDisposable where TStartup : class
    {
        private readonly TestServer _server;

        public TestFixture()
        {
            var builder = new WebHostBuilder().UseStartup<TStartup>();
            _server = new TestServer(builder);

            var configuration = TestsHelper.GetIConfigurationRoot();

            Client = _server.CreateClient();
        }

        public HttpClient Client { get; }
        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();
        }
    }
}
