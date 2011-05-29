using System.Web.Mvc;
using System.Web.Routing;

namespace i18n
{
    /// <summary>
    /// The entrypoint for internationalization features
    /// </summary>
    public class I18N
    {
        /// <summary>
        /// The default language for all localized keys; when a PO database
        /// is built, the default key file is stored at this locale location
        /// </summary>
        public static string DefaultTwoLetterISOLanguageName { get; set; }

        static I18N()
        {
            DefaultTwoLetterISOLanguageName = "en";
        }

        /// <summary>
        /// Registers the calling web application for automatic language
        /// URL routing based on the existing PO database
        /// </summary>
        public static void Register()
        {
            GlobalFilters.Filters.Add(new LanguageFilter());
            ApplyDecoratorToRoutes();
        }

        private static void ApplyDecoratorToRoutes()
        {
            var routes = RouteTable.Routes;
            using(routes.GetReadLock())
            {
                for (var i = 0; i < routes.Count; i++)
                {
                    routes[i] = new LanguageRouteDecorator(routes[i]);
                }
            }
        }
    }
}