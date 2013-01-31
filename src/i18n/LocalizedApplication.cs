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
