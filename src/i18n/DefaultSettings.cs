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
        }
    }
}