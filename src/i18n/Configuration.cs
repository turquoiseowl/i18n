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
        /// is built, the default key file is stored at this locale location
        /// </summary>
        public static LanguageTag DefaultTwoLetterISOLanguageTag { get; set; }

        /// <summary>
        /// Specifies the type of HTTP redirect to be issued by automatic language routing:
        /// true for 301 (permanent) redirects; false for 302 (temporary) ones.
        /// Defaults to false.
        /// </summary>
        public static bool PermanentRedirects { get; set; }

        static LocalizedApplication()
        {
            DefaultTwoLetterISOLanguageTag = LanguageTag.GetCachedInstance("en");
            PermanentRedirects = false;
            Container = new Container();
            Container.Register<ILocalizingService>(r => new LocalizingService());
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
    }
}
