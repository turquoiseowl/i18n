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
                // BUGBUG: the above two lines are redundant: already done by DefaultSettings static constructor.

            DefaultSettings.HtmlStringFormatter = new MvcHtmlStringFormatter();
        }

        /// <summary>
        /// Initializes i18n to work with MVC.
        /// </summary>
        public static void Register()
        {
            // NB: the original functionality of this is moved to RouteLocalization.Enable.
            // The only reason to leave this in now is to cause the static constructor to
            // be called, and all that really does is init the DefaultSettings.HtmlStringFormatter setting.
            // That in turn is only used at present by the DataAnnotations classes and there is no
            // mention in the docs that to use these you need to call this method.
            // TODO: look for way to initialize DefaultSettings.HtmlStringFormatter some other way
            // and so obviate the need for client to call this method.
            // This class then becomes redundant, at least until other MVC init stuff is required.
        }
    }
}
