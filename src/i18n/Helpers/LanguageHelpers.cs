using System.Collections.Concurrent;

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
        public static ConcurrentDictionary<string, LanguageTag> GetAppLanguages()
        {
            return LocalizedApplication.Current.TextLocalizerForApp.GetAppLanguages();
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
        public static LanguageTag GetMatchingAppLanguage(string langtag, int maxPasses = -1)
        {
            return GetMatchingAppLanguage(LanguageItem.ParseHttpLanguageHeader(langtag), maxPasses);
        }
        public static LanguageTag GetMatchingAppLanguage(LanguageItem[] languages, int maxPasses = -1)
        {
            LanguageTag lt = null;
            LocalizedApplication.Current.TextLocalizerForApp.GetText(null, null, languages, out lt, maxPasses);
            return lt;
        }

        /// <summary>
        /// Helper for translating the passed string entity which may contain zero or more fully-formed nugget,
        /// which may be called when an HttpContext is not available e.g. in background jobs.
        /// </summary>
        /// <remarks>
        /// When translating entities when handling HTTP requests, the language to use is selected from the current UserLanguages value
        /// which is derived from the HttpContext (that is, the request headers). In a background job, however,
        /// we don't normally have an HttpContext. With this method, however, you can save the UserLanguages 
        /// obtained from <see cref="i18n.HttpContextExtensions.ParseAndTranslate(System.Web.HttpContextBase, string)"/> when it is available, 
        /// i.e. prior to scheduling the background job, and pass it through to the job and then on to this method.
        /// Note that the UserLanguages array value can be persisted as a string using the <see cref="LanguageItem.DehydrateLanguageItemsToString"/>
        /// helper and converted back to an array using the <see cref="LanguageItem.HydrateLanguageItemsFromString"/> helper.
        /// Note that this method does not support the localization of URLs embedded in the entity.
        /// </remarks>
        /// <param name="userLanguages">
        /// A list of language preferences, sorted in order or preference (most preferred first).
        /// </param>
        /// <param name="entity">
        /// Entity to be processed. E.g HTTP response entity or Javascript file.
        /// </param>
        /// <returns>
        /// Processed (and possibly modified) entity.
        /// </returns>
        /// <seealso cref="i18n.HttpContextExtensions.ParseAndTranslate(System.Web.HttpContextBase, string)"/>
        public static string ParseAndTranslate(LanguageItem[] userLanguages, string entity)
        {
        // For impl. notes see ResponseFilter.Flush().
        //
            INuggetLocalizer nuggetLocalizer = LocalizedApplication.Current.NuggetLocalizerForApp;
            if (nuggetLocalizer != null) {
                entity = LocalizedApplication.Current.NuggetLocalizerForApp.ProcessNuggets(
                    entity,
                    userLanguages); }
            return entity;
        }

        /// <summary>
        /// Helper for translating the passed string entity which may contain zero or more fully-formed nugget,
        /// which may be called when an HttpContext is not available e.g. in background jobs.
        /// </summary>
        /// <remarks>
        /// When translating entities when handling HTTP requests, the language to use is selected from the current UserLanguages value
        /// which is derived from the HttpContext (that is, the request headers). In a background job, however,
        /// we don't normally have an HttpContext. With this method, however, you can save the UserLanguages 
        /// obtained from <see cref="i18n.HttpContextExtensions.ParseAndTranslate(System.Web.HttpContextBase, string)"/> when it is available, 
        /// i.e. prior to scheduling the background job, and pass it through to the job and then on to this method.
        /// Note that the UserLanguages array value can be persisted as a string using the <see cref="LanguageItem.DehydrateLanguageItemsToString"/>
        /// helper and converted back to an array using the <see cref="LanguageItem.HydrateLanguageItemsFromString"/> helper.
        /// Note that this method does not support the localization of URLs embedded in the entity.
        /// </remarks>
        /// <param name="userLanguages">
        /// A list of language preferences, sorted in order or preference (most preferred first)
        /// in compact string form. See <see cref="LanguageItem.DehydrateLanguageItemsToString"/>.
        /// </param>
        /// <param name="entity">
        /// Entity to be processed. E.g HTTP response entity or Javascript file.
        /// </param>
        /// <returns>
        /// Processed (and possibly modified) entity.
        /// </returns>
        /// <seealso cref="i18n.HttpContextExtensions.ParseAndTranslate(System.Web.HttpContextBase, string)"/>
        public static string ParseAndTranslate(string userLanguages, string entity)
        {
            return ParseAndTranslate(LanguageItem.HydrateLanguageItemsFromString(userLanguages), entity);
        }
    }
}
