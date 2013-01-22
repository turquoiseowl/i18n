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

namespace i18n
{
    /// <summary>
    /// A service for retrieving localized text from PO resource files
    /// </summary>
    public class LocalizingService : ILocalizingService, ILocalizingServiceEnhanced
    {

    // [ILocalizingService]

        /// <summary>
        /// Returns the best matching language for this application's resources, based the provided languages
        /// </summary>
        /// <param name="languages">A sorted list of language preferences</param>
        public virtual string GetBestAvailableLanguageFrom(string[] languages)
        {
            foreach (var language in languages)
            {
                var culture = GetCultureInfoFromLanguage(language);
                if (culture == null)
                {
                    continue;
                }
                
                // en-US
                var result = GetLanguageIfAvailable(culture.IetfLanguageTag);
                if(result != null)
                {
                    return result;
                }

                // Don't process the same culture code again
                if (culture.IetfLanguageTag.Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // en
                result = GetLanguageIfAvailable(culture.TwoLetterISOLanguageName);
                if (result != null)
                {
                    return result;
                }
            }

            return DefaultSettings.DefaultTwoLetterISOLanguageName;
        }

        /// <summary>
        /// Returns localized text for a given default language key, or the default itself,
        /// based on the provided languages and application resources
        /// </summary>
        /// <param name="key">The default language key to search for</param>
        /// <param name="languages">A sorted list of language preferences</param>
        /// <returns></returns>
        public virtual string GetText(string key, string[] languages)
        {
            // Prefer 'en-US', then 'en', before moving to next language choice
            foreach (var language in languages)
            {
                var culture = GetCultureInfoFromLanguage(language);
                if (culture == null)
                {
                    continue;
                }

                // Save cycles processing beyond the default; just return the original key
                if (culture.TwoLetterISOLanguageName.Equals(DefaultSettings.DefaultTwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
                {
                    return key;
                }

                // E.g. en-US
                var regional = TryGetTextFor(culture.IetfLanguageTag, key);
                if (regional != null)
                {
                    return regional;
                }

                // Now that region-specific lookup failed...try a region-neutral lookup. E.g. fr-CH -> fr.
                if (!culture.IetfLanguageTag.Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
                {
                    var neutral = TryGetTextFor(culture.TwoLetterISOLanguageName, key);
                    if (neutral != null)
                    {
                        return neutral;
                    }
                }
            }

            return key;
        }

    // [ILocalizingServiceEnhanced]

        /// <summary>
        /// Returns the best matching language for this application's resources, based the provided languages
        /// </summary>
        /// <param name="languages">A sorted list of language preferences</param>
        public virtual string GetBestAvailableLanguageFrom(LanguageItem[] languages)
        {
            foreach (var language in languages)
            {
                var culture = GetCultureInfoFromLanguage(language.ToString());
                if (culture == null)
                {
                    continue;
                }
                
                // en-US
                var result = GetLanguageIfAvailable(culture.IetfLanguageTag);
                if(result != null)
                {
                    return result;
                }

                // Don't process the same culture code again
                if (culture.IetfLanguageTag.Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // en
                result = GetLanguageIfAvailable(culture.TwoLetterISOLanguageName);
                if (result != null)
                {
                    return result;
                }
            }

            return DefaultSettings.DefaultTwoLetterISOLanguageName;
        }

        /// <summary>
        /// Returns localized text for a given default language key, or the default itself,
        /// based on the provided languages and application resources
        /// </summary>
        /// <param name="key">The default language key to search for</param>
        /// <param name="languages">A sorted list of language preferences</param>
        /// <returns></returns>
        public virtual string GetText(string key, LanguageItem[] languages, out LanguageTag o_langtag)
        {
            // Perform language matching based on UserLanguaes, AppLanguages, and presence of
            // resource under key for any particular AppLanguage.
            string text;
            o_langtag = LanguageMatching.MatchLists(languages, GetAppLanguages(), key, TryGetTextFor, out text);
            if (text != null) {
                return text; }

            // Next try default language.
            o_langtag = DefaultSettings.DefaultTwoLetterISOLanguageTag;
            return TryGetTextFor(DefaultSettings.DefaultTwoLetterISOLanguageName, key);
        }

    // Implementation

        private static readonly object Sync = new object();

        /// <summary>
        /// Obtains collection of language tags describing the set of languages for which one or more 
        /// resource are possibly defined.
        /// Note that the AppLanguages collection is unordered; this is because there is no innate 
        /// precedence at the resource level: precedence is only relevant to UserLanguages.
        /// </summary>
        private ConcurrentBag<LanguageTag> GetAppLanguages()
        {
            ConcurrentBag<LanguageTag> AppLanguages = (ConcurrentBag<LanguageTag>)HttpRuntime.Cache["i18n.AppLanguages"];
            if (AppLanguages != null) {
                return AppLanguages; }
            lock (Sync)
            {
                AppLanguages = (ConcurrentBag<LanguageTag>)HttpRuntime.Cache["i18n.AppLanguages"];
                if (AppLanguages != null) {
                    return AppLanguages; }
                AppLanguages = new ConcurrentBag<LanguageTag>();

               // Insert into cache.
               // NB: we do this before actually populating the collection. This is so that any changes to the
               // folders before we finish populating the collection will cause the cache item to be invalidated
               // and hence reloaded on next request, and so will not be missed.
                string directory;
                string path;
                GetDirectoryAndPath(null, out directory, out path);
                HttpRuntime.Cache.Insert("i18n.AppLanguages", AppLanguages, new CacheDependency(directory));

               // Populate the collection.
                List<string> dirs = new List<string>(Directory.EnumerateDirectories(directory));
                foreach (var dir in dirs)
                {
                    string langtag = Path.GetFileName(dir);
                    if (!IsPoFileNotValid(langtag)) {
                        AppLanguages.Add(LanguageTag.GetCachedInstance(langtag)); }
                }
               // Done.
                return AppLanguages;
            }
        }

        /// <returns>null if not available.</returns>
        private static string GetLanguageIfAvailable(string culture)
        {
        // Note that there is no need to serialize access to HttpRuntime.Cache when just reading from it.
        //
            ConcurrentDictionary<string, I18NMessage> messages = (ConcurrentDictionary<string, I18NMessage>)HttpRuntime.Cache[GetCacheKey(culture)];

            // If messages not yet loaded in for the language
            if (messages == null)
            {
                // Attempt to load messages, and if failed (because PO file doesn't exist)
                if (!LoadMessages(culture))
                {
                    // Avoid shredding the disk looking for non-existing files
                    CreateEmptyMessages(culture);
                    return null;
                }

                // Address messages just loaded.
                messages = (ConcurrentDictionary<string, I18NMessage>)HttpRuntime.Cache[GetCacheKey(culture)];
            }

            // The language is considered to be available if one or more message strings exist.
            return messages.Count > 0 ? culture : null;
        }

        /// <returns>null if not found.</returns>
        private static string TryGetTextFor(string culture, string key)
        {
            return GetLanguageIfAvailable(culture) != null ? LookupText(culture, key) : null;
        }

        private static void CreateEmptyMessages(string culture)
        {
            lock (Sync)
            {
                string directory;
                string path;
                GetDirectoryAndPath(culture, out directory, out path);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using(var fs = File.CreateText(path))
                {
                    fs.Flush();
                }

                // Cache messages.
                // NB: if the file changes we want to be able to rebuild the index without recompiling.
                HttpRuntime.Cache.Insert(GetCacheKey(culture), new ConcurrentDictionary<string, I18NMessage>(), new CacheDependency(path));

                // Reset any cached AppLanguages collection which is dependent on the set of loaded messages.
                HttpRuntime.Cache.Remove("i18n.AppLanguages");
            }
        }

        private static bool LoadMessages(string culture)
        {
            string directory;
            string path;
            GetDirectoryAndPath(culture, out directory, out path);

            if (!File.Exists(path))
            {
                return false;
            }

            LoadFromDiskAndCache(culture, path);
            return true;
        }

        /// <param name="culture">
        /// Null to get path of local directory only.
        /// </param>
        /// <param name="directory"></param>
        /// <param name="path"></param>
        private static void GetDirectoryAndPath(string culture, out string directory, out string path)
        {
            if (culture == null) {
                directory = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "locale");
                path = "";
            }
            else {
                directory = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "locale");
                directory = Path.Combine(directory, culture);
                path = Path.Combine(directory, "messages.po");
            }
        }

        private static bool IsPoFileNotValid(string culture)
        {
            string directory;
            string path;
            GetDirectoryAndPath(culture, out directory, out path);
            FileInfo fi = new FileInfo(path);
            if (!fi.Exists
                || fi.Length == 0) {
                return true; }
           // If messages not yet loaded...we don't know whether there are message or not so we can't say whether
           // valid or not.
            ConcurrentDictionary<string, I18NMessage> messages = (ConcurrentDictionary<string, I18NMessage>)HttpRuntime.Cache[GetCacheKey(culture)];
            if (messages == null) {
                return false; }
           //
            return messages.Count == 0;
        }

        private static void LoadFromDiskAndCache(string culture, string path)
        {
            lock (Sync)
            {
                // It is possible for multiple threads to race to this method. The first to
                // enter the above lock will insert the messages into the cache.
                // If we lost the race...no need to duplicate the work of the winning thread.
                if (HttpRuntime.Cache[GetCacheKey(culture)] != null) {
                    return; }

                using (var fs = File.OpenText(path))
                {
                    // http://www.gnu.org/s/hello/manual/gettext/PO-Files.html

                    var messages = new ConcurrentDictionary<string, I18NMessage>();
                    string line;
                    while ((line = fs.ReadLine()) != null)
                    {
                        if (line.StartsWith("#~"))
                        {
                            continue;
                        }

                        var message = new I18NMessage();
                        var sb = new StringBuilder();

                        if (line.StartsWith("#"))
                        {
                            sb.Append(CleanCommentLine(line));
                            while((line = fs.ReadLine()) != null && line.StartsWith("#"))
                            {
                                sb.Append(CleanCommentLine(line));
                            }
                            message.Comment = sb.ToString();

                            sb.Clear();
                            ParseBody(fs, line, sb, message);

                            // Only if a msgstr (translation) is provided for this entry do we add an entry to the cache.
                            // This conditions facilitates more useful operation of the GetLanguageIfAvailable method,
                            // which prior to this condition was indicating a language was available when in fact there
                            // were zero translation in the PO file (it having been autogenerated during gettext merge).
                            if (!string.IsNullOrWhiteSpace(message.MsgStr))
                            {
                                if (!messages.ContainsKey(message.MsgId))
                                {
                                    messages[message.MsgId] = message;
                                }
                            }
                        }
                        else if (line.StartsWith("msgid"))
                        {
                            ParseBody(fs, line, sb, message);
                        }
                    }

                    // Cache messages.
                    // NB: if the file changes we want to be able to rebuild the index without recompiling.
                    HttpRuntime.Cache.Insert(GetCacheKey(culture), messages, new CacheDependency(path));

                    // Reset any cached AppLanguages collection which is dependent on the set of loaded messages.
                    HttpRuntime.Cache.Remove("i18n.AppLanguages");

                }
            }
        }

        private static void ParseBody(TextReader fs, string line, StringBuilder sb, I18NMessage message)
        {
            if(!string.IsNullOrEmpty(line))
            {
                if(line.StartsWith("msgid"))
                {
                    var msgid = line.Unquote();
                    sb.Append(msgid);

                    while ((line = fs.ReadLine()) != null && !line.StartsWith("msgstr") && (msgid = line.Unquote()) != null)
                    {
                        sb.Append(msgid);
                    }

                    message.MsgId = sb.ToString().Unescape();
                }

                sb.Clear();
                if(!string.IsNullOrEmpty(line) && line.StartsWith("msgstr"))
                {
                    var msgstr = line.Unquote();
                    sb.Append(msgstr);

                    while ((line = fs.ReadLine()) != null && (msgstr = line.Unquote()) != null)
                    {
                        sb.Append(msgstr);
                    }

                    message.MsgStr = sb.ToString().Unescape();
                }
            }
        }

        private static string CleanCommentLine(string line)
        {
            return line.Replace("# ", "").Replace("#. ", "").Replace("#: ", "").Replace("#, ", "").Replace("#| ", "");
        }

        /// <returns>null if not found.</returns>
        private static string LookupText(string culture, string key)
        {
        // Note that there is no need to serialize access to HttpRuntime.Cache when just reading from it.
        //
            var messages = (ConcurrentDictionary<string, I18NMessage>) HttpRuntime.Cache[GetCacheKey(culture)];
            I18NMessage message = null;

            if (messages == null || !messages.TryGetValue(key, out message))
            {
                return null;
            }

            return message.MsgStr;
                // We check this for null/empty before adding to collection.
        }

        /// <returns>null if not found.</returns>
        private static CultureInfo GetCultureInfoFromLanguage(string language)
        {
            //var semiColonIndex = language.IndexOf(';');
            //return semiColonIndex > -1
            //           ? new CultureInfo(language.Substring(0, semiColonIndex), true)
            //           : new CultureInfo(language, true);
            //Codes wouldn't work on ie10 of some languages of Windows 8 and Windows 2012

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

        private static string GetCacheKey(string culture)
        {
            //return string.Format("po:{0}", culture).ToLowerInvariant();
                // The above will cause a new string to be allocated.
                // So subsituted with the following code.

            // Obtain the cache key without allocating a new string (except for first time).
            // As this method is a high-frequency method, the overhead in the (lock-free) dictionary 
            // lookup is thought to outweigh the potentially large number of temporary string allocations
            // and the consequently hastened garbage collections.
            return LanguageTag.GetCachedInstance(culture).GlobalKey;
        }
    }
}
