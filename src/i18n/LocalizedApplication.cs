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

        static LocalizedApplication()
        {
            DefaultLanguage = ("en");
            Container = new Container();
            Container.Register<ILocalizingService>(r => new LocalizingService());
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

    }
}
