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
    public class LocalizingService : ILocalizingService
    {

    #region [ILocalizingService]

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
                string directory;
                string path;
                GetDirectoryAndPath(null, out directory, out path);
                HttpRuntime.Cache.Insert("i18n.AppLanguages", AppLanguages, new FsCacheDependency(directory));

               // Populate the collection.
                List<string> dirs = new List<string>(Directory.EnumerateDirectories(directory));
                foreach (var dir in dirs)
                {
                    string langtag = Path.GetFileName(dir);
                    if (IsLanguageValid(langtag)) {
                        AppLanguages[langtag] = LanguageTag.GetCachedInstance(langtag); }
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
                text = TryGetTextFor(LocalizedApplication.DefaultLanguageTag.ToString(), key);
                if (text != null) {
                    o_langtag = LocalizedApplication.DefaultLanguageTag;
                    return text; }
            }

            return null;
        }

    #endregion

        private static readonly object Sync = new object();

        /// <summary>
        /// Assesses whether a language is PO-valid, that is whether or not one or more
        /// localized messages exists for the language.
        /// </summary>
        /// <returns>true if one or more localized messages exist for the language; otherwise false.</returns>
        private static bool IsLanguageValid(string langtag)
        {
        // Note that there is no need to serialize access to HttpRuntime.Cache when just reading from it.
        //
            ConcurrentDictionary<string, PoMessage> messages = (ConcurrentDictionary<string, PoMessage>)HttpRuntime.Cache[GetCacheKey(langtag)];

            // If messages not yet loaded in for the language
            if (messages == null)
            {
                // Attempt to load messages, and if failed (because PO file doesn't exist)
                if (!LoadMessages(langtag))
                {
                    // Avoid shredding the disk looking for non-existing files
                    CreateEmptyMessages(langtag);
                    return false;
                }

                // Address messages just loaded.
                messages = (ConcurrentDictionary<string, PoMessage>)HttpRuntime.Cache[GetCacheKey(langtag)];
            }

            // The language is considered to be available if one or more message strings exist.
            return messages.Count > 0;
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
        private static string TryGetTextFor(string langtag, string key)
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
            if (string.Compare(langtag, LocalizedApplication.DefaultLanguageTag.ToString(), true) == 0) {
                return key; }

            // Lookup failed.
            return null;
        }

        private static void CreateEmptyMessages(string langtag)
        {
            lock (Sync)
            {
                string directory;
                string path;
                GetDirectoryAndPath(langtag, out directory, out path);

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
                HttpRuntime.Cache.Insert(GetCacheKey(langtag), new ConcurrentDictionary<string, PoMessage>(), new CacheDependency(path));
            }
        }

        private static bool LoadMessages(string langtag)
        {
            string directory;
            string path;
            GetDirectoryAndPath(langtag, out directory, out path);

            if (!File.Exists(path))
            {
                return false;
            }

            LoadFromDiskAndCache(langtag, path);
            return true;
        }

        /// <param name="langtag">
        /// Null to get path of local directory only.
        /// </param>
        /// <param name="directory"></param>
        /// <param name="path"></param>
        private static void GetDirectoryAndPath(string langtag, out string directory, out string path)
        {
            if (langtag == null) {
                directory = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "locale");
                path = "";
            }
            else {
                directory = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "locale");
                directory = Path.Combine(directory, langtag);
                path = Path.Combine(directory, "messages.po");
            }
        }

        private static void LoadFromDiskAndCache(string langtag, string path)
        {
            lock (Sync)
            {
                // It is possible for multiple threads to race to this method. The first to
                // enter the above lock will insert the messages into the cache.
                // If we lost the race...no need to duplicate the work of the winning thread.
                if (HttpRuntime.Cache[GetCacheKey(langtag)] != null) {
                    return; }

                using (var fs = File.OpenText(path))
                {
                    // http://www.gnu.org/s/hello/manual/gettext/PO-Files.html

                    var messages = new ConcurrentDictionary<string, PoMessage>();
                    string line;
                    while ((line = fs.ReadLine()) != null)
                    {
                        if (line.StartsWith("#~"))
                        {
                            continue;
                        }

                        var message = new PoMessage();
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
                            // This conditions facilitates more useful operation of the IsLanguageValid method,
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
                    HttpRuntime.Cache.Insert(GetCacheKey(langtag), messages, new CacheDependency(path));
                }
            }
        }

        private static void ParseBody(TextReader fs, string line, StringBuilder sb, PoMessage message)
        {
            if (!string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("msgid"))
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
                if (!string.IsNullOrEmpty(line) && line.StartsWith("msgstr"))
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
        private static string LookupText(string langtag, string key)
        {
        // Note that there is no need to serialize access to HttpRuntime.Cache when just reading from it.
        //
            var messages = (ConcurrentDictionary<string, PoMessage>) HttpRuntime.Cache[GetCacheKey(langtag)];
            PoMessage message = null;

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
