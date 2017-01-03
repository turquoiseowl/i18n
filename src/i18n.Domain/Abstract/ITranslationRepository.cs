using System.Collections.Generic;
using System.Web.Caching;
    // TODO: this above dependency is unfortunate and should be removed.
    // That would involve a reworking of the design for notifications
    // of languages being modified.
    // GetCacheDependencyForSingleLanguage could be replaced with an event in the Translation
    // object which is signalled when that particular translation becomes dirty.
    // Like wise GetCacheDependencyForAllLanguages could return just an event.
    // It is then down to the client to wrap these events in a custom CacheDependency
    // that monitors the event.
using i18n.Domain.Entities;

namespace i18n.Domain.Abstract
{
    /// <summary>
    /// For managing a translation repository for reading, writing and searching. As long as you implement this you can store your translation wherever you want. Db/ po files/xml
    /// </summary>
    public interface ITranslationRepository
    {
        /// <summary>
        /// Retrieves a translation with all items (both with translation set and not)
        /// </summary>
        /// <param name="langtag">The language tag to get the translation for. For instance "sv-SE"</param>
        /// <param name="fileNames">A list of file names generated.</param>
        /// <param name="loadingCache">Flag determining whether the call came from the generator or the localizing module.</param>
        /// <returns>A Translation object with the Language->LanguageShortTag set and all the translation items returned in a Dictionary</returns>
        Translation GetTranslation(string langtag, List<string> fileNames = null, bool loadingCache = true);

        /// <summary>
        /// Gets all available languages. There is a setting for available languages that can be used by the implementation. But the implementation can if prefered use other method.
        /// </summary>
        /// <returns>List of <see cref="Language"/> with a minimum of LanguageShortTag set</returns>
        IEnumerable<Language> GetAvailableLanguages();

        /// <summary>
        /// Checks if a translation exists. This can either use settings as <see cref="GetAvailableLanguages()" /> or a custom implementation 
        /// </summary>
        /// <param name="langtag">The language tag to check for, like "sv-SE"</param>
        /// <returns>True of the language exists, otherwise false</returns>
        bool TranslationExists(string langtag);

        /// <summary>
        /// Saves a translation (persisting it). How this is done is completely up to the implementation. As long as the same language can be loaded with <see cref="GetTranslation"/>
        /// </summary>
        /// <param name="translation">The translation to save. At minimum the Items and Language->LanguageShortTag must be set</param>
        void SaveTranslation(Translation translation);

        /// <summary>
        /// Save a template. A template differs from a translation in that a translation holds all messages but no translation data. It is used for updating translations to make sure all translations have all strings.
        /// </summary>
        /// <param name="items">All template items to save, in a dictionary indexed by their id</param>
        /// <returns>True if the template have been saved</returns>
        bool SaveTemplate(IDictionary<string, TemplateItem> items);

        /// <summary>
        /// Returns a CacheDependency for a language. This can be a subclass such as <see cref="SqlCacheDependency"/>
        /// This is used to remove caches when a languages has been updated
        /// </summary>
        /// <param name="langtag">The language tag to get a dependency for</param>
        /// <returns>The dependency for the language sent in.</returns>
        CacheDependency GetCacheDependencyForSingleLanguage(string langtag);  

        /// <summary>
        /// Returns a CacheDependecy for all languages, that is to say if there has been an additon or removal of one or more languages.
        /// Just like <see cref="GetCacheDependencyForSingleLanguage(string)"/> we can return a subclass of CacheDependency as required.
        /// </summary>
        /// <returns>The cache dependency for the list of languages.</returns>
        CacheDependency GetCacheDependencyForAllLanguages();
    }
}
