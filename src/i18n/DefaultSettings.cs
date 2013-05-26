using container;

namespace i18n
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultSettings
    {

        internal static Container Container { get; set; }

        /// <summary>
        /// The default language for all localized keys; when a PO database
        /// is built, the default key file is stored at this locale location
        /// </summary>
        public static string DefaultTwoLetterISOLanguageName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        static DefaultSettings()
        {
            DefaultTwoLetterISOLanguageName = "en";
            Container = new Container();
            Container.Register<ILocalizingService>(r => new LocalizingService());
        }

        /// <summary>
        /// 
        /// </summary>
        public static IHtmlStringFormatter HtmlStringFormatter
        {
            get
            {
                return Container.Resolve<IHtmlStringFormatter>();
            }
            set
            {
                Container.Remove<IHtmlStringFormatter>();
                Container.Register(r => value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static ILocalizingService LocalizingService
        {
            get
            {
                return Container.Resolve<ILocalizingService>();
            }
            set
            {
                Container.Remove<ILocalizingService>();
                Container.Register(r => value);
            }
        }
    }
}