using System.Web.Mvc;
using System.Web.Routing;

namespace i18n
{
    internal abstract class RouteDecoratorBase<T> : RouteBase, IRouteWithArea where T : RouteDecoratorBase<T>
    {
        protected RouteBase _route;

        protected RouteDecoratorBase(RouteBase route)
        {
            _route = route;
        }

        public RouteBase InnerRoute
        {
            get { return _route; }
        }

        #region IRouteWithArea Members

        public string Area
        {
            get
            {
                var r = _route;
                while (r is T)
                {
                    r = ((T)r).InnerRoute;
                }
                var s = GetAreaToken(r);
                return s;
            }
        }

        #endregion
        
        private static string GetAreaToken(RouteBase r)
        {
            var route = r as Route;
            if (route != null && route.DataTokens != null && route.DataTokens.ContainsKey("area"))
            {
                return (route.DataTokens["area"] as string);
            }
            return null;
        }
    }
}
