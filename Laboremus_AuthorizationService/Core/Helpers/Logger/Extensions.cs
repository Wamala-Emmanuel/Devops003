using System.Linq;

namespace Laboremus_AuthorizationService.Core.Helpers.Logger
{
    public static class Extensions  
    {
        public static string ToLowerCamelCase(this string str)
        {
            return str.First().ToString().ToLower() + str.Substring(1);
        }
    }
}
