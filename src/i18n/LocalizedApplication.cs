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
            Container = new Container();
            Container.Register<ILocalizingService>(r => new LocalizingService());
            Container.Register<INuggetLocalizer>(r => new NuggetLocalizer());
            Container.Register<IEarlyUrlLocalizer>(r => new EarlyUrlLocalizer());
            Container.Register<IUrlLocalizer>(r => new UrlLocalizer());
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
        /// Gets or sets the current IEarlyUrlLocalizer implementation used by i18n route localization.
        /// </summary>
        /// <remarks>
        /// Setting this interface implicity enables or disables the respective feacture.
        /// This feature depends on the LocalizedModule HTTP module being enabled in web.config.
        /// By default, the interface is set to the default implementation.
        /// </remarks>
        public static IEarlyUrlLocalizer EarlyUrlLocalizer
        {
            get { return Container.Resolve<IEarlyUrlLocalizer>(); }
            set
            {
                Container.Remove<IEarlyUrlLocalizer>();
                Container.Register(r => value);
            }
        }

        /// <summary>
        /// Gets or sets the current INuggetLocalizer implementation used by i18n route localization.
        /// </summary>
        /// <remarks>
        /// Setting this interface implicity enables or disables the respective feacture.
        /// This feature depends on the LocalizedModule HTTP module being enabled in web.config.
        /// By default, the interface is set to the default implementation.
        /// </remarks>
        public static INuggetLocalizer NuggetLocalizer
        {
            get { return Container.Resolve<INuggetLocalizer>(); }
            set
            {
                Container.Remove<INuggetLocalizer>();
                Container.Register(r => value);
            }
        }

        /// <summary>
        /// Gets or sets the current IUrlLocalizer implementation used by i18n route localization.
        /// </summary>
        /// <remarks>
        /// This interface is used by the default EarlyUrlLocalizer and NuggetLocalizer implementations.
        /// [Deprecated] It is also used by the MVC RouteLocalization implementation.
        /// By default, the interface is set to the default implementation.
        /// </remarks>
        public static IUrlLocalizer UrlLocalizer
        {
            get { return Container.Resolve<IUrlLocalizer>(); }
            set
            {
                Container.Remove<IUrlLocalizer>();
                Container.Register(r => value);
            }
        }
    }
}
