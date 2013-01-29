using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace i18n
{
    public static class LanguageHelpers
    {
        /// <summary>
        /// Obtains collection of language tags describing the set of Po-valid languages, that
        /// is the languages for which one or more resource are defined.
        /// Note that the AppLanguages collection is unordered; this is because there is no innate 
        /// precedence at the resource level: precedence is only relevant to UserLanguages.
        /// </summary>
        /// <remarks>Requires i18n to be in Enhanced mode.</remarks>
        public static ConcurrentDictionary<string, LanguageTag> GetAppLanguages()
        {
            if (DefaultSettings.LocalizingServiceEnhanced == null) {
                throw new System.InvalidOperationException("Expected Enhanced mode."); }

            return DefaultSettings.LocalizingServiceEnhanced.GetAppLanguages();
        }

        /// <summary>
        /// Attempts to match the passed language with an AppLanguage.
        /// </summary>
        /// <param name="langtag">The subject language to match, typically a UserLanguage.</param>
        /// <param name="maxPasses">
        /// 0 - allow exact match only
        /// 1 - allow exact match or default-region match only
        /// 2 - allow exact match or default-region match or script match only
        /// 3 - allow exact match or default-region match or script match or language match only
        /// 4 - allow exact match or default-region match or script or language match only, or failing return the default language.
        /// -1 to set to most tolerant (i.e. 4).
        /// </param>
        /// <returns>
        /// A language tag identifying an AppLanguage that will be the same as, or related langtag.
        /// </returns>
        /// <remarks>Requires i18n to be in Enhanced mode.</remarks>
        public static LanguageTag GetMatchingAppLanguage(string langtag, int maxPasses = -1)
        {
            return GetMatchingAppLanguage(LanguageItem.ParseHttpLanguageHeader(langtag), maxPasses);
        }
        public static LanguageTag GetMatchingAppLanguage(LanguageItem[] languages, int maxPasses = -1)
        {
            if (DefaultSettings.LocalizingServiceEnhanced == null) {
                throw new System.InvalidOperationException("Expected Enhanced mode."); }

            LanguageTag lt = null;
            DefaultSettings.LocalizingServiceEnhanced.GetText(null, languages, out lt, maxPasses);
            return lt;
        }
    }
}
