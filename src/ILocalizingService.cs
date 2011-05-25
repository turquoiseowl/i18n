namespace i18n
{
    /// <summary>
    /// Defines a service for retrieving localized text from data source
    /// </summary>
    public interface ILocalizingService
    {
        /// <summary>
        /// Returns the best matching language for this application's resources, based the provided languages
        /// </summary>
        /// <param name="languages">A sorted list of language preferences</param>
        string GetBestAvailableLanguageFrom(string[] languages);

        /// <summary>
        /// Returns localized text for a given default language key, or the default itself,
        /// based on the provided languages and application resources
        /// </summary>
        /// <param name="key">The default language key to search for</param>
        /// <param name="languages">A sorted list of language preferences</param>
        /// <returns></returns>
        string GetText(string key, string[] languages);
    }
}