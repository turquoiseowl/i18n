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
using i18n.Domain.Concrete;

namespace i18n
{
    /// <summary>
    /// A service for retrieving localized text from PO resource files
    /// </summary>
    public class TextLocalizer : ITextLocalizer
    {
        private i18nSettings _settings;

	    private ITranslationRepository _translationRepository;

	    public TextLocalizer(
            i18nSettings settings,
            ITranslationRepository translationRepository)
	    {
            _settings = settings;
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
                HttpRuntime.Cache.Insert("i18n.AppLanguages", AppLanguages, _translationRepository.GetCacheDependencyForAllLanguages());

               // Populate the collection.
	            List<string> languages = _translationRepository.GetAvailableLanguages().Select(x => x.LanguageShortTag).ToList();

                // Ensure default language is included in AppLanguages where appropriate.
                if (LocalizedApplication.Current.MessageKeyIsValueInDefaultLanguage
                    && !languages.Any(x => LocalizedApplication.Current.DefaultLanguageTag.Equals(x))) {
                    languages.Add(LocalizedApplication.Current.DefaultLanguageTag.ToString()); }

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

        public virtual string GetText(string msgid, string msgcomment, LanguageItem[] languages, out LanguageTag o_langtag, int maxPasses = -1)
        {
            // Validate arguments.
            if (maxPasses > (int)LanguageTag.MatchGrade._MaxMatch +1) { 
                maxPasses = (int)LanguageTag.MatchGrade._MaxMatch +1; }
            // Init.
            bool fallbackOnDefault = maxPasses == (int)LanguageTag.MatchGrade._MaxMatch +1
                || maxPasses == -1;
            // Determine the key for the msg lookup. This may be either msgid or msgid+msgcomment, depending on the prevalent
            // MessageContextEnabledFromComment setting.
            string msgkey = msgid == null ? 
                msgid:
                TemplateItem.KeyFromMsgidAndComment(msgid, msgcomment, _settings.MessageContextEnabledFromComment);
            // Perform language matching based on UserLanguages, AppLanguages, and presence of
            // resource under msgid for any particular AppLanguage.
            string text;
            o_langtag = LanguageMatching.MatchLists(
                languages, 
                GetAppLanguages(), 
                msgkey, 
                TryGetTextFor, 
                out text, 
                Math.Min(maxPasses, (int)LanguageTag.MatchGrade._MaxMatch));
            // If match was successfull
            if (text != null) {
                // If the msgkey was returned...don't output that but rather the msgid as the msgkey
                // may be msgid+msgcomment.
                if (text == msgkey) {
                    return msgid; }                
                return text;
            }
            // Optionally try default language.
            if (fallbackOnDefault)
            {
                o_langtag = LocalizedApplication.Current.DefaultLanguageTag;
                return msgid;
            }
            //
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
            // Default language is always valid.
            if (LocalizedApplication.Current.MessageKeyIsValueInDefaultLanguage
                && LocalizedApplication.Current.DefaultLanguageTag.Equals(langtag)) {
                return true; }

			ConcurrentDictionary<string, TranslationItem> messages = (ConcurrentDictionary<string, TranslationItem>)HttpRuntime.Cache[GetCacheKey(langtag)];

            // If messages not yet loaded in for the language
            if (messages == null)
            {
	            return _translationRepository.TranslationExists(langtag);
            }

	        return true;
        }

        /// <summary>
        /// Lookup whether any messages exist for the passed langtag, and if so attempts
        /// to lookup the message for the passed msgid, or if the msgid is null returns indication
        /// of whether any messages exist for the langtag.
        /// </summary>
        /// <param name="langtag">
        /// Language tag of the subject langtag.
        /// </param>
        /// <param name="msgkey">
        /// Key of the message to lookup, or null to test for any message loaded for the langtag.
        /// When on-null, the format of the key is as generated by the TemplateItem.KeyFromMsgidAndComment
        /// helper.
        /// </param>
        /// <returns>
        /// On success, returns the translated message, or if msgkey is null returns an empty string ("")
        /// to indciate that one or more messages exist for the langtag.
        /// On failure, returns null.
        /// </returns>
        private string TryGetTextFor(string langtag, string msgkey)
        {
            // If no messages loaded for language...fail.
            if (!IsLanguageValid(langtag)) {
                return null; }

            // If not testing for a specific message, that is just testing whether any messages 
            // are present...return positive.
            if (msgkey == null) {
                return ""; }   

            // Lookup specific message text in the language PO and if found...return that.
            string text = LookupText(langtag, msgkey);
            if (text != null) {
                return text; }

            // If we are looking up in the default language, and the message keys describe values
            // in that language...return the msgkey.
            if (LocalizedApplication.Current.DefaultLanguageTag.Equals(langtag)
                && LocalizedApplication.Current.MessageKeyIsValueInDefaultLanguage) {
                return msgkey; }

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

				Translation t = _translationRepository.GetTranslation(langtag);

				// Cache messages.
				// NB: if the file changes we want to be able to rebuild the index without recompiling.
                // NB: it is possible for GetCacheDependencyForSingleLanguage to return null in the
                // case of the default language where it is added to AppLanguages yet doesn't actually exist.
                // See MessageKeyIsValueInDefaultLanguage.
                var cd = _translationRepository.GetCacheDependencyForSingleLanguage(langtag);
				if (cd != null) {
                    HttpRuntime.Cache.Insert(GetCacheKey(langtag), t.Items, cd); }
			}
            return true;
        }

       /// <returns>null if not found.</returns>
        private string LookupText(string langtag, string msgkey)
        {
        // Note that there is no need to serialize access to HttpRuntime.Cache when just reading from it.
        //
            var messages = (ConcurrentDictionary<string, TranslationItem>) HttpRuntime.Cache[GetCacheKey(langtag)];
            TranslationItem message = null;

			//we need to populate the cache
			if (messages == null)
			{
				LoadMessagesIntoCache(langtag);
                messages = (ConcurrentDictionary<string, TranslationItem>)HttpRuntime.Cache[GetCacheKey(langtag)];
			}

           // Normalize any CRLF in the msgid i.e. to just LF.
           // PO only support LF so we expect strings to be stored in the repo in that form.
           // NB: we test Contains before doing Replace in case string.Replace allocs a new
           // string even on no change. (This method is called very often.)
            if (msgkey.Contains("\r\n")) {
                msgkey = msgkey.Replace("\r\n", "\n"); }

            if (messages == null
                || !messages.TryGetValue(msgkey, out message)
                || !message.Message.IsSet())
            {
                return null;
            }

            return message.Message;
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
