using container;

namespace i18n
{
    public class DefaultSettings
    {
        /// <summary>
        /// Modes of operation supported by i18n.
        /// </summary>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/windows/apps/jj673578.aspx"/>
        public enum Mode
        {
            /// <summary>
            /// Original language matching and selection algorithm.
            /// </summary>
            Basic,

            /// <summary>
            /// Multi-pass language matching algorithm that provides more granular matching 
            /// of a client's preferred languages against available application languages.
            /// Requires the localizing service to implement ILocalizingServiceEnhanced.
            /// Also, multi-facet language selection algorithm based on langtag in URL path prefix
            /// (e.g. example.com/fr-CA/account/signup), then cookies ("i18n.langtag"),
            /// then user agent Accept-Language setting (language-matched against list of AppLanguages),
            /// and finally the default App language.
            /// </summary>
            Enhanced,
        }

        /// <summary>
        /// The language matching algorithm to be used.
        /// Defaults to LanguageMatching.Basic but may be changed by app.
        /// </summary>
        public static Mode TheMode { get; set; }

        /// <summary>
        /// The default language for all localized keys; when a PO database
        /// is built, the default key file is stored at this locale location
        /// </summary>
        public static string DefaultTwoLetterISOLanguageName { get; set; }
        public static LanguageTag DefaultTwoLetterISOLanguageTag { get; set; }

        static DefaultSettings()
        {
            TheMode = Mode.Basic;
            DefaultTwoLetterISOLanguageName = "en";
            DefaultTwoLetterISOLanguageTag = LanguageTag.GetCachedInstance(DefaultTwoLetterISOLanguageName);
            Container = new Container();
            Container.Register<ILocalizingService>(r => new LocalizingService());
            Container.Register<ILocalizingServiceEnhanced>(r => new LocalizingService());
        }

        internal static Container Container { get; set; }
        
        public static IHtmlStringFormatter HtmlStringFormatter
        {
            get { return Container.Resolve<IHtmlStringFormatter>(); }
            set
            {
                Container.Remove<IHtmlStringFormatter>();
                Container.Register(r => value);
            }
        }

        public static ILocalizingService LocalizingService
        {
            get { return Container.Resolve<ILocalizingService>(); }
            set
            {
                Container.Remove<ILocalizingService>();
                Container.Register(r => value);
            }
        }

        public static ILocalizingServiceEnhanced LocalizingServiceEnhanced
        {
            get { return Container.Resolve<ILocalizingServiceEnhanced>(); }
            set
            {
                Container.Remove<ILocalizingServiceEnhanced>();
                Container.Register(r => value);
            }
        }
    }
}
