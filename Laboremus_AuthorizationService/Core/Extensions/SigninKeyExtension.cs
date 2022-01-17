using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laboremus_AuthorizationService.Core.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class SigninKeyExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public static IIdentityServerBuilder AddCertificateFromFile(this IIdentityServerBuilder builder, IConfiguration options,
            ILogger<Startup> logger)
        {
            var keyFilePath = options[KeyFilePath];
            var keyFilePassword = options[KeyFilePassword];

            if (File.Exists(keyFilePath))
            {
                logger.LogDebug($"SigninCredentialExtension adding key from file {keyFilePath}");
                builder.AddSigningCredential(new X509Certificate2(keyFilePath, keyFilePassword));
            }
            else
            {
                logger.LogDebug($"SigninCredentialExtension cannot find key file {keyFilePath}");
            }

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        public static string KeyFilePassword => "SigninKeyCredentials:KeyFilePassword";

        /// <summary>
        /// 
        /// </summary>
        public static string KeyFilePath => "SigninKeyCredentials:KeyFilePath";
    }
}
