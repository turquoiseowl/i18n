namespace i18n
{
    /// <summary>
    /// Abstracts a service for performing background translations i.e. outside the context 
    /// of an HTTP request. Specifically including support over ITranslateSvc
    /// for the specifying of user language preferences.
    /// </summary>
    public interface IBackgroundTranslateSvc
    {
        /// <summary>
        /// Returns the translation of the passed string entity which may contain zero or more fully-formed nuggets.
        /// </summary>
        /// <param name="entity">
        /// String containing zero or more fully-formed nuggets which are to be translated.
        /// </param>
        /// <param name="userLanguages">
        /// A list of language preferences, sorted in order or preference (most preferred first)
        /// in compact string form.
        /// May be null/empty string in which case a a single-item language item array 
        /// representing a null PAL is returned.
        /// Example values:
        ///     "fr-CA;q=1,fr;q=0.5"
        ///     "en-CA;q=2,de;q=0.5,en;q=1,fr-FR;q=0,ga;q=0.5"
        ///     "en-CA;q=1,de;q=0.5,en;q=1,fr-FR;q=0,ga;q=0.5"
        ///     "en-CA;q=1"
        ///     "?;q=2"
        ///     "?;q=2,de;q=0.5,en;q=1,fr-FR;q=0,ga;q=0.5"
        ///     ""
        /// See <see cref="i18n.LanguageHelpers.ParseAndTranslate(string, string)"/>.
        /// See <see cref="HttpContextExtensions.GetRequestUserLanguagesAsString"/>.
        /// See <see cref="LanguageItem.DehydrateLanguageItemsToString"/>.
        /// </param>
        /// <returns>
        /// Localized (translated) entity.
        /// </returns>
        string ParseAndTranslate(string entity, string userLanguages = null);
    }
}
