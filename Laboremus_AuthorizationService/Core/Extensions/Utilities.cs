using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Laboremus_AuthorizationService.Core.Extensions
{
    public static class Utilities
    {
        public static string IsActive(this IHtmlHelper html,
            string control,
            string action)
        {
            var routeData = html.ViewContext.RouteData;

            var routeAction = (string)routeData.Values["action"];
            var routeControl = (string)routeData.Values["controller"];

            // both must match
            var returnActive = control == routeControl &&
                               action == routeAction;

            return returnActive ? "active" : "";
        }

        public static bool IsValidEmailAddress(this string str)
        {
            return str != null && (new[] {"@", "."}).All(str.Contains);
        }
    }
}
