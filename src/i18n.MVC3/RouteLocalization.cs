using System.Web.Mvc;
using System.Web.Routing;
using container;

namespace i18n
{
    /// <summary>
    /// Describes the configuration of the i18n MVC Route Localization feature.
    /// </summary>
    public static class RouteLocalization
    {
        internal static Container Container { get; set; }

        public static bool Enabled { get; set; }
        
        static RouteLocalization()
        {
            Container = new Container();
            Container.Register<IUrlLocalizer>(r => new UrlLocalizer());
            Enabled = false;
        }

        /// <summary>
        /// Gets or sets the current IUrlLocalizer implementation used by i18n route localization.
        /// </summary>
        public static IUrlLocalizer UrlLocalizer
        {
            get { return Container.Resolve<IUrlLocalizer>(); }
            set
            {
                Container.Remove<IUrlLocalizer>();
                Container.Register(r => value);
            }
        }

        /// <summary>
        /// Enables automatic language URL routing based on the existing PO database.
        /// </summary>
        public static void Enable()
        {
            // Only enable the once.
            if (Enabled) {
                return; }
            Enabled = true;

            GlobalFilters.Filters.Add(new LanguageFilter());

            ApplyDecoratorToRoutes();
        }

        private static void ApplyDecoratorToRoutes()
        {
            var routes = RouteTable.Routes;
            using (routes.GetReadLock())
            {
                for (var i = 0; i < routes.Count; i++)
                {
                    RouteBase route = routes[i];
                    if (!route.NoLocalize())
                    {
                        DebugHelpers.WriteLine("I18N.ApplyDecoratorToRoutes -- decorating route: {0}", route.ToString());
                        routes[i] = new LanguageRouteDecorator(route);
                    }
                }
            }
        }
    }
}
