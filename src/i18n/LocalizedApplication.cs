using System;
using System.Text.RegularExpressions;
using container;

namespace i18n
{
    /// <summary>
    /// Manages the configuration of the i18n features of your localized application.
    /// </summary>
    public class LocalizedApplication
    {
        /// <summary>
        /// The default language for all localized keys; when a PO database
        /// is built, the default key file is stored at this locale location.
        /// </summary>
        /// <remarks>
        /// Supports a subset of BCP 47 language tag spec corresponding to the Windows
        /// support for language names, namely the following subtags:
        ///     language (mandatory, 2 alphachars)
        ///     script   (optional, 4 alphachars)
        ///     region   (optional, 2 alphachars | 3 decdigits)
        /// Example tags supported:
        ///     "en"            [language]
        ///     "en-US"         [language + region]
        ///     "zh"            [language]
        ///     "zh-HK"         [language + region]
        ///     "zh-123"        [language + region]
        ///     "zh-Hant"       [language + script]
        ///     "zh-Hant-HK"    [language + script + region]
        /// </remarks>
        public static string DefaultLanguage { 
            get {
                return DefaultLanguageTag.ToString();
            }
            set {
                DefaultLanguageTag = LanguageTag.GetCachedInstance(value);
            }
        }
        public static LanguageTag DefaultLanguageTag { get; set; }

        /// <summary>
        /// Specifies the type of HTTP redirect to be issued by automatic language routing:
        /// true for 301 (permanent) redirects; false for 302 (temporary) ones.
        /// Defaults to false.
        /// </summary>
        public static bool PermanentRedirects { get; set; }

        /// <summary>
        /// Specifies whether Early URL Localization is to be enabled.
        /// </summary>
        /// <remarks>
        /// Defaults to true. This feature requires the LocalizedModule HTTP module to be intalled in web.config.
        /// <see cref="!:https://docs.google.com/drawings/d/1cH3_PRAFHDz7N41l8Uz7hOIRGpmgaIlJe0fYSIOSZ_Y/edit?usp=sharing"/>
        /// </remarks>
        public static bool EnableEarlyUrlLocalization { get; set; }

        /// <summary>
        /// Regular expression that controls the ContextTypes elligible for Late URL Localization.
        /// </summary>
        /// <remarks>
        /// Set to null to disable Late URL Localization. Defaults to text/html and 
        /// application/javascript. Client may customise this member, for instance in Application_Start.
        /// This feature requires the LocalizedModule HTTP module to be intalled in web.config.
        /// </remarks>
        public static Regex ContentTypesToLocalize = new Regex("^(?:text/html|application/javascript)$");

        static LocalizedApplication()
        {
            DefaultLanguage = ("en");
            PermanentRedirects = false;
            EnableEarlyUrlLocalization = true;
            Container = new Container();
            Container.Register<ILocalizingService>(r => new LocalizingService());
            Container.Register<IUrlLocalizer>(r => new UrlLocalizer());
            Container.Register<INuggetLocalizer>(r => new NuggetLocalizer());
        }

        internal static Container Container { get; set; }
        
        public static ILocalizingService LocalizingService
        {
            get { return Container.Resolve<ILocalizingService>(); }
            set
            {
                Container.Remove<ILocalizingService>();
                Container.Register(r => value);
            }
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
        /// Gets or sets the current INuggetLocalizer implementation used by i18n route localization.
        /// </summary>
        public static INuggetLocalizer NuggetLocalizer
        {
            get { return Container.Resolve<INuggetLocalizer>(); }
            set
            {
                Container.Remove<INuggetLocalizer>();
                Container.Register(r => value);
            }
        }
    }
}
