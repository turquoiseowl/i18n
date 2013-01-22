using container;

namespace i18n
{
    public class DefaultSettings
    {
        /// <summary>
        /// Enumeration of supported language matching algorithms.
        /// </summary>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/windows/apps/jj673578.aspx"/>
        public enum LanguageMatching
        {
            /// <summary>
            /// Original language matching algorithm.
            /// </summary>
            Basic,

            /// <summary>
            /// Multi-pass language matching algorithm that provides more granular matching 
            /// of a client's preferred languages against available application languages.
            /// Requires the localizing service to implement ILocalizingServiceEnhanced.
            /// </summary>
            Enhanced,
        }

        /// <summary>
        /// The language matching algorithm to be used.
        /// Defaults to LanguageMatching.Basic but may be changed by app.
        /// </summary>
        public static LanguageMatching DefaultLanguageMatchingAlgorithm { get; set; }

        /// <summary>
        /// The default language for all localized keys; when a PO database
        /// is built, the default key file is stored at this locale location
        /// </summary>
        public static string DefaultTwoLetterISOLanguageName { get; set; }
        public static LanguageTag DefaultTwoLetterISOLanguageTag { get; set; }

        static DefaultSettings()
        {
            DefaultLanguageMatchingAlgorithm = LanguageMatching.Basic;
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
