using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using i18n.Domain.Abstract;
using i18n.Domain.Entities;

namespace i18n
{
    /// <summary>
    /// A service for retrieving localized text from PO resource files
    /// </summary>
    public class TextLocalizer : ITextLocalizer
    {
	    private ITranslationRepository _translationRepository;

	    public TextLocalizer(ITranslationRepository translationRepository)
	    {
		    _translationRepository = translationRepository;
	    }


	    #region [ITextLocalizer]

        public virtual ConcurrentDictionary<string, LanguageTag> GetAppLanguages()
        {
            ConcurrentDictionary<string, LanguageTag> AppLanguages = (ConcurrentDictionary<string, LanguageTag>)HttpRuntime.Cache["i18n.AppLanguages"];
            if (AppLanguages != null) {
                return AppLanguages; }
            lock (Sync)
            {
                AppLanguages = (ConcurrentDictionary<string, LanguageTag>)HttpRuntime.Cache["i18n.AppLanguages"];
                if (AppLanguages != null) {
                    return AppLanguages; }
                AppLanguages = new ConcurrentDictionary<string, LanguageTag>();

               // Insert into cache.
               // NB: we do this before actually populating the collection. This is so that any changes to the
               // folders before we finish populating the collection will cause the cache item to be invalidated
               // and hence reloaded on next request, and so will not be missed.
                HttpRuntime.Cache.Insert("i18n.AppLanguages", AppLanguages, _translationRepository.GetCacheDependencyAllLanguages());

               // Populate the collection.
	            List<string> languages = _translationRepository.GetAvailableLanguages().Select(x => x.LanguageShortTag).ToList();
                foreach (var langtag in languages)
                {
					if (IsLanguageValid(langtag))
					{
                        AppLanguages[langtag] = LanguageTag.GetCachedInstance(langtag); 
					}
                }

               // Done.
                return AppLanguages;
            }
        }

        public virtual string GetText(string key, LanguageItem[] languages, out LanguageTag o_langtag, int maxPasses = -1)
        {
            // Validate arguments.
            if (maxPasses > (int)LanguageTag.MatchGrade._MaxMatch +1) { 
                maxPasses = (int)LanguageTag.MatchGrade._MaxMatch +1; }
            // Init.
            bool fallbackOnDefault = maxPasses == (int)LanguageTag.MatchGrade._MaxMatch +1
                || maxPasses == -1;
            string text;
            // Perform language matching based on UserLanguaes, AppLanguages, and presence of
            // resource under key for any particular AppLanguage.
            o_langtag = LanguageMatching.MatchLists(
                languages, 
                GetAppLanguages(), 
                key, 
                TryGetTextFor, 
                out text, 
                Math.Min(maxPasses, (int)LanguageTag.MatchGrade._MaxMatch));
            if (text != null) {
                return text; }

            // Optionally try default language.
            if (fallbackOnDefault)
            {
                text = TryGetTextFor(LocalizedApplication.Current.DefaultLanguageTag.ToString(), key);
                if (text != null) {
                    o_langtag = LocalizedApplication.Current.DefaultLanguageTag;
                    return text; }
            }

            return null;
        }

    #endregion

        internal readonly object Sync = new object();

        /// <summary>
        /// Assesses whether a language is PO-valid, that is whether or not one or more
        /// localized messages exists for the language.
        /// </summary>
        /// <returns>true if one or more localized messages exist for the language; otherwise false.</returns>
        private bool IsLanguageValid(string langtag)
        {
        // Note that there is no need to serialize access to HttpRuntime.Cache when just reading from it.
        //
			ConcurrentDictionary<string, TranslateItem> messages = (ConcurrentDictionary<string, TranslateItem>)HttpRuntime.Cache[GetCacheKey(langtag)];

            // If messages not yet loaded in for the language
            if (messages == null)
            {
	            return _translationRepository.TranslationExists(langtag);
            }

	        return true;
        }

        /// <summary>
        /// Lookup whether any messages exist for the passed langtag, and if so attempts
        /// to lookup the message for the passed key, or if the key is null returns indication
        /// of whether any messages exist for the langtag.
        /// </summary>
        /// <param name="langtag">
        /// Language tag of the subject langtag.
        /// </param>
        /// <param name="key">
        /// Key (msgid) of the message to lookup, or null to test for any message loaded for the langtag.
        /// </param>
        /// <returns>
        /// On success, returns the translated message, or if key is null returns an empty string ("")
        /// to indciate that one or more messages exist for the langtag.
        /// On failure, returns null.
        /// </returns>
        private string TryGetTextFor(string langtag, string key)
        {
            // If no messages loaded for language...fail.
            if (!IsLanguageValid(langtag)) {
                return null; }

            // If not testing for a specific message, that is just testing whether any messages 
            // are present...return positive.
            if (key == null) {
                return ""; }   

            // Lookup specific message text in the language PO and if found...return that.
            string text = LookupText(langtag, key);
            if (text != null) {
                return text; }

            // If the language is the default language, by definition the text always exists
            // and as there isn't a translation defined for the key, we return the key itself.
            if (string.Compare(langtag, LocalizedApplication.Current.DefaultLanguageTag.ToString(), true) == 0) {
                return key; }

            // Lookup failed.
            return null;
        }

        private bool LoadMessagesIntoCache(string langtag)
        {
			lock (Sync)
			{
				// It is possible for multiple threads to race to this method. The first to
				// enter the above lock will insert the messages into the cache.
				// If we lost the race...no need to duplicate the work of the winning thread.
				if (HttpRuntime.Cache[GetCacheKey(langtag)] != null)
				{
					return true;
				}

				Translation t = _translationRepository.GetLanguage(langtag);

				// Cache messages.
				// NB: if the file changes we want to be able to rebuild the index without recompiling.
				HttpRuntime.Cache.Insert(GetCacheKey(langtag), t.Items, _translationRepository.GetCacheDependencyLanguage(langtag));
			}
            return true;
        }

       /// <returns>null if not found.</returns>
        private string LookupText(string langtag, string key)
        {
        // Note that there is no need to serialize access to HttpRuntime.Cache when just reading from it.
        //
            var messages = (ConcurrentDictionary<string, TranslateItem>) HttpRuntime.Cache[GetCacheKey(langtag)];
            TranslateItem message = null;

			//we need to populate the cache
			if (messages == null)
			{
				LoadMessagesIntoCache(langtag);
			}


            if (messages == null || !messages.TryGetValue(key, out message))
            {
                return null;
            }

            return message.Message;
                // We check this for null/empty before adding to collection.
        }

        /// <returns>null if not found.</returns>
        private static CultureInfo GetCultureInfoFromLanguage(string language)
        {
        // TODO: replace usage of CultureInfo with the LanguageTag class.
        // This method and the use of CultureInfo is now surpassed by the LanguageTag class,
        // thus making this method of handling language tags redundant.
        //
            if (string.IsNullOrWhiteSpace(language)) {
                return null; }
            try {
                var semiColonIndex = language.IndexOf(';');
                language = semiColonIndex > -1 ? language.Substring(0, semiColonIndex) : language;
                language = System.Globalization.CultureInfo.CreateSpecificCulture(language).Name;
                return new CultureInfo(language, true);
            }
            catch (Exception) {
                return null; }
        }

        private static string GetCacheKey(string langtag)
        {
            //return string.Format("po:{0}", langtag).ToLowerInvariant();
                // The above will cause a new string to be allocated.
                // So subsituted with the following code.

            // Obtain the cache key without allocating a new string (except for first time).
            // As this method is a high-frequency method, the overhead in the (lock-free) dictionary 
            // lookup is thought to outweigh the potentially large number of temporary string allocations
            // and the consequently hastened garbage collections.
            return LanguageTag.GetCachedInstance(langtag).GlobalKey;
        }
    }
}
