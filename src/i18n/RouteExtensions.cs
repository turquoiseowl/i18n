using System.Web;
using System.Web.Routing;

namespace i18n
{
    public static class RouteExtensions
    {
        /// <summary>
        /// Helper for determining whether a route is NOT to be localized.
        /// </summary>
        /// <remarks>
        /// Route is not to be decorated if it is of type System.Web.Routing.Route and contains 
        /// a datatoken called "nolocalize" (of any value).
        /// </remarks>
        /// <param name="routeBase">Subject route.</param>
        /// <returns>true if route is NOT to be localized; false if it is.</returns>
        public static bool NoLocalize(this System.Web.Routing.RouteBase routeBase)
        {
            Route route = routeBase as Route;
            if (route == null
                || route.DataTokens == null) {
                return false; }
            return route.DataTokens.ContainsKey("nolocalize");
        }
    }
}
