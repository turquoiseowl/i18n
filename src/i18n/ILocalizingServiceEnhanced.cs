namespace i18n
{
    /// <summary>
    /// Defines a service for retrieving localized text from a data source using the
    /// DefaultSettings.LanguageMatching.Enhanced algorithm.
    /// </summary>
    public interface ILocalizingServiceEnhanced
    {
        /// <summary>
        /// Looks up and returns localized text for a resource id using the
        /// DefaultSettings.LanguageMatching.Enhanced algorithm.
        /// </summary>
        /// <param name="key">
        /// Idenfities the msgid of the subject resource.
        /// Null if we are not interested in a particular resource but wish to know
        /// the best matching language for which ANY resources are available (one or more).
        /// </param>
        /// <param name="languages">
        /// A list of language preferences, sorted in order or preference (most preferred first).
        /// </param>
        /// <param name="o_langtag">
        /// On success, outputs a description of the language from which the resource was selected.
        /// </param>
        /// <returns>
        /// When key is set to non-null, returns either the sucessully-looked up localized string, or 
        /// null if the lookup failed.
        /// When key is set to null, returns null.
        /// </returns>
        string GetText(string key, LanguageItem[] languages, out LanguageTag o_langtag);
    }
}
