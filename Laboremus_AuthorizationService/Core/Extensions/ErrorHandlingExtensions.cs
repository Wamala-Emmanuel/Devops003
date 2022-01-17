using Microsoft.AspNetCore.Builder;

namespace Laboremus_AuthorizationService.Core.Extensions
{
    public static class ErrorHandlingExtensions
    {
        /// <summary>
        /// Insert error handling middleware
        /// </summary>
        /// <param name="builder">IApplication Builder extension</param>    
        /// <returns></returns>
        public static IApplicationBuilder UseCustomErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandling>();
        }
    }
}
