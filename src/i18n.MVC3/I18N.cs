using System.Web.Mvc;
using System.Web.Routing;

namespace i18n
{
    /// <summary>
    /// The entrypoint for internationalization features in ASP.NET MVC
    /// </summary>
    public class I18N
    {
        public static string DefaultTwoLetterISOLanguageName
        {
            get { return DefaultSettings.DefaultTwoLetterISOLanguageName; }
            set { DefaultSettings.DefaultTwoLetterISOLanguageName = value; }
        }

        static I18N()
        {
            DefaultTwoLetterISOLanguageName = "en";
            DefaultSettings.LocalizingService = new LocalizingService();
            DefaultSettings.HtmlStringFormatter = new MvcHtmlStringFormatter();
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
            using (routes.GetReadLock())
            {
                for (var i = 0; i < routes.Count; i++)
                {
                    routes[i] = new LanguageRouteDecorator(routes[i]);
                }
            }
        }
    }
}