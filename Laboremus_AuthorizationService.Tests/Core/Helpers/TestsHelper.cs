using System.Collections.Generic;
using Bogus;
using Laboremus_AuthorizationService.DTOs;
using Laboremus_AuthorizationService.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Laboremus_AuthorizationService.Tests.Core.Helpers
{
    public static class TestsHelper
    {
        public static IConfigurationRoot GetIConfigurationRoot()
        {
            var mockEnvironment = new Mock<IHostingEnvironment>().Object;

            mockEnvironment.EnvironmentName = "Development";
            
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public static ILogger<T> GetLogger<T>()
        {
            return new Mock<ILogger<T>>().Object;
        }

        /// <summary>
        /// Generates an export request object for unit testing
        /// </summary>
        public static ExportRequest GetExportRequest()
        {
            var requestFaker = new Faker<ExportRequest>()
                .RuleFor(r => r.Id, f => f.Random.Guid())
                .RuleFor(r => r.GenerationStatus, f => ExportStatus.Complete)
                .RuleFor(r => r.CreatedOn, f => f.Date.Past().ToUniversalTime())
                .RuleFor(r => r.DownloadedOn, f => f.Date.Past().ToUniversalTime())
                .RuleFor(r => r.FileName, (f, r) => $"{r.Id}.csv")
                .RuleFor(r => r.IsDeleted, f => f.Random.Bool());
            return requestFaker.Generate();
        }

        /// <summary>
        /// Returns a UserExportRequest for unit tests 
        /// </summary>
        public static UserExportRequest GetUserExportRequest()
        {
            return new UserExportRequest
            {
                
            };
        }

        /// <summary>
        /// Returns export settings to use for unit testing 
        /// </summary>
        public static ExportSettings GetExportSettings()
        {
            return new ExportSettings
            {
                Buffer = 1024,
                DelayInHours = 2,
                DaysBack = 3,
                FolderPath = "folder",
                PageSize = 10,
                Offset = 3,
                RequestLimit = 1000000
            };;
        }

        /// <summary>
        /// Generates a list of requests export 
        /// </summary>
        public static List<ExportRequest> GetListOfExportRequests(int count = 20)
        {
            var requestFaker = new Faker<ExportRequest>()
                .RuleFor(r => r.Id, f => f.Random.Guid())
                .RuleFor(r => r.GenerationStatus, f => f.PickRandom<ExportStatus>())
                .RuleFor(r => r.CreatedOn, f => f.Date.Past(1).ToUniversalTime())
                .RuleFor(r => r.FileName, (f, r) => $"{r.Id}.csv")
                .RuleFor(r => r.IsDeleted, f => f.Random.Bool());
            return requestFaker.Generate(count);
        }
    }
}
