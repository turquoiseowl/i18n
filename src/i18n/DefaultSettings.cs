using container;

namespace i18n
{
    public class DefaultSettings
    {
        /// <summary>
        /// The default language for all localized keys; when a PO database
        /// is built, the default key file is stored at this locale location
        /// </summary>
        public static string DefaultTwoLetterISOLanguageName { get; set; }
        static DefaultSettings()
        {
            DefaultTwoLetterISOLanguageName = "en";
            Container = new Container();
            Container.Register<ILocalizingService>(r => new LocalizingService());
            
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
    }
}