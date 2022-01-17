#pragma warning disable CS1591 // Missing XML comment
using System.DirectoryServices.AccountManagement;
using Microsoft.Extensions.Configuration;

namespace Laboremus_AuthorizationService.Core.Extensions
{
    public static class ConfigExtensions
    {
        public static string RemoveTrailingSlash(this string str)
        {
            return str.TrimEnd('/');
        }

        public static string GetAuthServer(this IConfiguration configuration)
        {
            return configuration["BaseUrl"];
        }  

        public static string GetAdDomain(this IConfiguration configuration)
        {
            return configuration["OnPremiseAD:Domain"];
        }

        public static IdentityType GetAdIdentityType(this IConfiguration configuration)
        {
            return (IdentityType)System.Enum.Parse(typeof(IdentityType), configuration["OnPremiseAD:IdentityType"]);
        }

        public static bool OnPremiseAdEnabled(this IConfiguration configuration)
        {
            return configuration.GetSection("OnPremiseAD:Enabled").Get<bool>();
        }

        public static bool GetSwaggerEnabled(this IConfiguration configuration)
        {
            return bool.Parse(configuration["ShowSwaggerDocs"] ?? "false");
        }

        public static bool GetPasswordEnabled(this IConfiguration configuration)
        {
            return bool.Parse(configuration["EnablePasswordLink"] ?? "false");
        }
    }
}