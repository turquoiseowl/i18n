using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace i18n
{
    /// <summary>
    /// Helper class for parsing and manipulating language tags.
    /// Supports a subset of BCP 47 language tag spec corresponding to the Windows
    /// support for language names, namely the following subtags:
    ///     language (mandatory, 2 alphachars)
    ///     script   (optional, 4 alphachars)
    ///     region   (optional, 2 alphachars | 3 decdigits)
    /// Example tags supported:
    ///     "en"            [language]
    ///     "en-US"         [language + region]
    ///     "zh"            [language]
    ///     "zh-HK"         [language + region]
    ///     "zh-123"        [language + region]
    ///     "zh-Hant"       [language + script]
    ///     "zh-Hant-HK"    [language + script + region]
    /// </summary>
    /// <seealso href="http://www.microsoft.com/resources/msdn/goglobal/default.mspx"/>
    public class LanguageTag : ILanguageTag, IEquatable<LanguageTag>
    {
    // Decl
        public static readonly string[,] NormalizedLangTags =
        {
            { "zh-CN", "zh-Hans" },
            { "zh-TW", "zh-Hant" },
        };
        public enum MatchGrade
        {
            /// <summary>
            /// Only consider a match where language and script and region parts match.
            /// E.g. fr matches fr
            /// E.g. zh-Hans-HK matches zh-Hans-HK
            /// </summary>
            ExactMatch = 0,
            /// <summary>
            /// Only consider a match where language and script parts match, one region is set and the other region 
            /// is not set.
            /// E.g. fr matches fr-BE
            /// E.g. zh-Hans-HK matches zh-Hans
            /// </summary>
            DefaultRegion = 1,
            /// <summary>
            /// Only consider a match where language and script parts match. Region part need not match.
            /// E.g. fr-CA matches fr-BE
            /// E.g. zh-Hant-HK matches zh-Hant-TW
            /// </summary>
            ScriptMatch = 2,
            /// <summary>
            /// Only consider a match where language matches. Script and region parts need not match.
            /// E.g. zh-Hans-HK matches zh
            /// </summary>
            LanguageMatch = 3,
            _MaxMatch = LanguageMatch,
        }
    // Data
        static readonly Regex m_regex_parseLangtag = new Regex(
            @"^([a-zA-Z]{2})(?:-([a-zA-Z]{4}))?(?:-([a-zA-Z]{2}|[0-9]{3}))?$", 
            RegexOptions.CultureInvariant);
            // ([a-zA-Z]{2})
            //      Matches language.
            // (?:-([a-zA-Z]{4}))?
            //      Matches script.
            //      NB: The inner group is wrapped in an outer non-capturing group that
            //      prefixed the former with the '-' which is thus not captured.
            // (?:-([a-zA-Z]{2}|[0-9]{3}))?
            //      Matches region.
            //      NB: The inner group is wrapped in an outer non-capturing group that
            //      prefixed the former with the '-' which is thus not captured.
        static readonly Regex m_regex_parseUrl = new System.Text.RegularExpressions.Regex(
            @"^/([a-zA-Z]{2}(?:-[a-zA-Z]{4})?(?:-(?:[a-zA-Z]{2}|[0-9]{3}))?)(?:$|/)", 
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
                // ^/
                // (                                # begin 1st and only capture group
                // [a-zA-Z]{2}                      # 2-letter country code
                // (?:-[a-zA-Z]{4})?                # optional script code - not a capture group itself
                // (?:-(?:[a-zA-Z]{2}|[0-9]{3}))?   # optional region code (2-letter or 3-digit) - not a capture group itself
                // )                                # end 1st and only capture group
                // (?:$|/)                          # match end of string or fwd-slash char - not a capture group itself
        private static ConcurrentDictionary<string, LanguageTag> s_cache = new ConcurrentDictionary<string, LanguageTag>();
            // Facilitates fast and efficient re-use of languag tag instances.
            // Key = langtag string.
            // Write-access to this member to be serialized via s_sync.
        private static readonly object s_sync = new object();
            // Facilitates serialization of write-access to s_cache.
    // Props
        /// <summary>
        /// Original full language tag string passed to constructor.
        /// </summary>
        private string m_langtag;
        /// <summary>
        /// Reference to any parent language tag, or null if no parent determined.
        /// The parent is the the language tag with one less subtag.
        /// E.g. if all three supported subtags are set (language, script and region), the parent 
        /// will be the language tag composed of the language and script subtags.
        /// </summary>
        LanguageTag m_parent;
        /// <summary>
        /// Mandatory Language subtag, or if CON fails then null.
        /// </summary>
        public string Language { get; private set; }
        /// <summary>
        /// Optional Script subtag.
        /// </summary>
        public string Script { get; private set; }
        /// <summary>
        /// Optional Region subtag.
        /// </summary>
        public string Region { get; private set; }
        /// <summary>
        /// Unique string per language which is suitable for using as a key in global
        /// caches such as HttpRuntime.Cache. Inited during construction.
        /// </summary>
        public string GlobalKey { get; private set; }
        /// <summary>
        /// Corresponding CultureInfo instance, or null if the langtag is unsupported on this system.
        /// </summary>
        public CultureInfo CultureInfo { get; private set; }
        /// <summary>
        /// If the system supports a cultureinfo object for the language, this is the native name of 
        /// the language suitable for user display, otherwise it is the language tag string.
        /// </summary>
        public string NativeNameTitleCase { get; private set; }
    // Con
        /// <summary>
        /// Constructs a new instance based on a language tag string.
        /// If successful, then the Language property is set to a valid language subtag.
        /// </summary>
        /// <param name="langtag">
        /// Supports a subset of BCP 47 language tag spec corresponding to the Windows
        /// support for language names, namely the following subtags:
        ///     language (mandatory, 2 alphachars)
        ///     script   (optional, 4 alphachars)
        ///     region   (optional, 2 alphachars | 3 decdigits)
        /// Example tags supported:
        ///     "en"            [language]
        ///     "en-US"         [language + region]
        ///     "zh"            [language]
        ///     "zh-HK"         [language + region]
        ///     "zh-123"        [language + region]
        ///     "zh-Hant"       [language + script]
        ///     "zh-Hant-HK"    [language + script + region]
        /// </param>
        /// <seealso href="http://www.microsoft.com/resources/msdn/goglobal/default.mspx"/>
        public LanguageTag(string langtag)
        {
            m_langtag = langtag.Trim();
           // Normalize certain langtags:
           // «LX113» http://www.w3.org/International/articles/language-tags/#script
            for (int i = 0; i < NormalizedLangTags.GetLength(0); ++i) {
                if (0 == string.Compare(m_langtag, NormalizedLangTags[i,0], true)) {
                    m_langtag = NormalizedLangTags[i,1];
                    break;
                }
            }
           // Parse the langtag.
            Match match = m_regex_parseLangtag.Match(m_langtag);
            if (match.Success
                && match.Groups.Count == 4) {
                Language = match.Groups[1].Value;
                Script   = match.Groups[2].Value;
                Region   = match.Groups[3].Value;
            }
           // Load any parent:
           //   l-s-r -> l-s
           //   l-r   -> l
           //   l-s   -> l
           //   l     -> no parent
            if (Region.IsSet() && Script.IsSet()) {
                m_parent = GetCachedInstance(string.Format("{0}-{1}", Language, Script)); }
            else if (Script.IsSet() || Region.IsSet()) {
                m_parent = GetCachedInstance(Language); }
            else {
                m_parent = null; }
           //
            GlobalKey = string.Format("po:{0}", m_langtag).ToLowerInvariant();
           //
            try {
                CultureInfo = new CultureInfo(langtag);
            }
            catch(System.ArgumentException) {}
           //
            NativeNameTitleCase = CultureInfo != null ? CultureInfo.TextInfo.ToTitleCase(CultureInfo.NativeName) : "m_langtag";
            //Debug.Assert(ToString() == langtag);
        }
        /// <summary>
        /// Instance factory that supports re-use of instances which by definition are read-only.
        /// </summary>
        /// <param name="langtag">
        /// Supports a subset of BCP 47 language tag spec corresponding to the Windows
        /// support for language names, namely the following subtags:
        ///     language (mandatory, 2 alphachars)
        ///     script   (optional, 4 alphachars)
        ///     region   (optional, 2 alphachars | 3 decdigits)
        /// Example tags supported:
        ///     "en"            [language]
        ///     "en-US"         [language + region]
        ///     "zh"            [language]
        ///     "zh-HK"         [language + region]
        ///     "zh-123"        [language + region]
        ///     "zh-Hant"       [language + script]
        ///     "zh-Hant-HK"    [language + script + region]
        /// </param>
        /// <returns>Either new or pre-exisiting instance, or null if langtag is invalid.</returns>
        /// <seealso href="http://www.microsoft.com/resources/msdn/goglobal/default.mspx"/>
        public static LanguageTag GetCachedInstance(string langtag)
        {
            if (!langtag.IsSet()) {
                return null; }
            LanguageTag result = null;
           // Get any extant instance, no need to lock as just reading.
            if (s_cache.TryGetValue(langtag, out result)) {
                return result; }
           // Instance doesn't exist so we may need to add which will require serialized access, so lock.
            lock (s_sync)
            {
               // Check again for instance incase another thd just created it.
                if (s_cache.TryGetValue(langtag, out result)) {
                    return result; }
               // If cache is over a certain size...clear it.
               // NB: this prevents the cache from filling with invalid langtag values,
               // possibly due to DOS attack.
                if (s_cache.Count > 10000) {
                    s_cache.Clear(); }
               // Create and cache new instance.
                result = new LanguageTag(langtag);
                if (!result.IsValid()) {
                    return null; }
                s_cache[langtag] = result;
                return result;
            }
        }
    // [Object]
        /// <returns>
        /// Language tag string.
        /// Supports a subset of BCP 47 language tag spec corresponding to the Windows
        /// support for language names, namely the following subtags:
        ///     language (mandatory, 2 alphachars)
        ///     script   (optional, 4 alphachars)
        ///     region   (optional, 2 alphachars | 3 decdigits)
        /// Example tags supported:
        ///     "en"            [language]
        ///     "en-US"         [language + region]
        ///     "zh"            [language]
        ///     "zh-HK"         [language + region]
        ///     "zh-123"        [language + region]
        ///     "zh-Hant"       [language + script]
        ///     "zh-Hant-HK"    [language + script + region]
        /// </returns>
        public override string ToString()
        {
            return m_langtag.IsSet() ? m_langtag : "";
        }
    // [ILanguageTag]
        string   ILanguageTag.GetLanguage()   { return Language; }
        string   ILanguageTag.GetExtlang()    { return null; }
        string   ILanguageTag.GetScript()     { return Script; }
        string   ILanguageTag.GetRegion()     { return Region; }
        string[] ILanguageTag.GetVariant()    { return null; }
        string   ILanguageTag.GetExtension()  { return null; }
        string   ILanguageTag.GetPrivateuse() { return null; }
        ILanguageTag ILanguageTag.GetParent() { return m_parent; }
        int      ILanguageTag.GetMaxParents() { return 2; }
        CultureInfo ILanguageTag.GetCultureInfo()      { return CultureInfo; }
        string   ILanguageTag.GetNativeNameTitleCase() { return NativeNameTitleCase; }
    // [IEquatable<ILanguageTag>]
        public bool Equals(ILanguageTag other)
        {
            return 0 == string.Compare(m_langtag, other.ToString(), true);
        }
    // [IEquatable<LanguageTag>]
        public bool Equals(LanguageTag other)
        {
            return 0 == string.Compare(m_langtag, other.m_langtag, true);
        }
    // Operations
        /// <summary>
        /// Performs 'language matching' between lang described by this (A)
        /// and language decibed by i_rhs (B). Essentially, returns an assessment of
        /// how well a speaker of A will understand B.
        /// The key points are as follows:
        ///   · The Script is almost as relevant as the language itself; that is, if
        ///     you speak a language but do not understand the script, you cannot
        ///     read that language. Thus a mismatch in Script should score low.
        ///   · The Region is less relevant than Script to understanding of language.
        ///     The one exception to this is where the Region has traditionally been
        ///     used to also indicate the Script. E.g.
        ///         zh-CH -> Chinese (Simplified)  i.e. zh-Hans
        ///         zh-TW -> Chinese (Traditional) i.e. zh-Hant
        ///     In these cases we normalize all legacy langtags to their new values
        ///     before matching. E.g. zh-CH is normalized to zh-Hans.
        /// «LX113»
        /// </summary>
        /// <param name="i_rhs"></param>
        /// <returns>
        /// Returns a score on to what extent the two languages match. The value ranges from
        /// 100 (exact match) down to 0 (fundamental language tag mismatch), with values 
        /// in between which may be used to compare quality of a match, larger the value
        /// meaning better quality.
        /// </returns>
        /// <remarks>
        /// Matching values:
        ///                                              RHS
        /// this                    lang    lang+script     lang+region     lang+script+region
        /// ----------------------------------------------------------------------------------
        /// lang                |   A       D               C               D
        /// lang+script         |   D       A               D               B
        /// lang+region         |   C       D               A               D
        /// lang+script+region  |   D       B               D               A
        /// 
        /// A. Exact match (100)
        ///     All three subtags match.
        /// B. Unbalanced Region Mismatch (99) [zh, zh-HK]
        ///     Language and Script match;
        ///     one side has Region set while the other doesn't.
        ///     Here there is the possibility that due to defaults Region matches.
        /// C. Balanced Region Mismatch (98) [zh-IK, zh-HK]
        ///     Language and Script match;
        ///     both sides have Region set but to different values.
        ///     Here there is NO possibility that Region matches.
        /// D. Unbalanced Script Mismatch (97) [zh-HK, zh-Hant-HK]
        ///     Language matches, Region may match;
        ///     one side has Script set while the other doesn't.
        ///     Here there is the possibility that due to defaults Script matches.
        /// E. Balanced Script Mismatch (96)
        ///     Language matches, Region may match;
        ///     both sides have Script set but to different values.
        ///     Here there is NO possibility that Script matches.
        /// F. Language Mismatch (0)
        ///     Language doesn't match.
        /// </remarks>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/windows/apps/jj673578.aspx"/>
        public int Match(LanguageTag i_rhs, MatchGrade matchGrade = MatchGrade.LanguageMatch)
        {
        //
            if (i_rhs == null) {
                throw new ArgumentNullException("i_rhs"); }
           // Init.
            bool[] L = { 0 == string.Compare(Language, i_rhs.Language, true), Language.IsSet(), i_rhs.Language.IsSet() };
            bool[] S = { 0 == string.Compare(Script  , i_rhs.Script  , true), Script  .IsSet(), i_rhs.Script  .IsSet() };
            bool[] R = { 0 == string.Compare(Region  , i_rhs.Region  , true), Region  .IsSet(), i_rhs.Region  .IsSet() };
            int score = 100;
           // Logic.
           // F.
            if (!L[0]) {
                return 0; }
           // A.
            if (S[0] && R[0]) {
                return score; }
            --score;
            if (matchGrade != MatchGrade.ExactMatch) {
               // B.
                if (S[0] && !R[0] && R[1] != R[2]) {
                    return score; }
                --score;
                if (matchGrade != MatchGrade.DefaultRegion) {
                   // C.
                    if (S[0] && !R[0] && R[1] == R[2]) {
                        return score; }
                    --score;
                    if (matchGrade != MatchGrade.ScriptMatch) {
                       // D.
                        if (!S[0] && S[1] != S[2]) {
                            return score; }
                        --score;
                       // E.
                        if (!S[0] && S[1] == S[2]) {
                            return score; }
                    }
                    //--score;
                    //DebugHelpers.WriteLine("LanguageTag.Match -- fallen through: {0}, {1}", ToString(), i_rhs.ToString());
                    //Debug.Assert(false);
                }
            }
           // F.
            return 0;
        }
        /// <summary>
        /// Looks up in the passed collection of supported AppLanguages the language that is best matched
        /// to this langtag. I.e. the written AppLanguage that a user understanding this langtag
        /// will most-likely understand.
        /// </summary>
        /// <returns>Selected CultureInfoEx instance from the AppLanguages collection or null if there was no match.</returns>
        public int Match(LanguageTag[] AppLanguages, MatchGrade matchGrade = MatchGrade.LanguageMatch)
        {
            int matchScore = 0;
            foreach(LanguageTag langtag in AppLanguages) {
                int score = Match(langtag, matchGrade);
                if (score > matchScore) {
                    matchScore = score;
                    if (matchScore == 100) { // Can't beat an exact match.
                        break; }
                }
            }
            return matchScore;
        }
        /// <summary>
        /// Helper for detecting a URL prefixed with a langtag part, and if found outputs
        /// both the langtag and the URL with the prefix removed.
        /// </summary>
        /// <remarks>
        /// This method does not check for the validity of the returned langtag other than
        /// it matching the pattern of a langtag as supported by this LanguageTag class.
        /// </remarks>
        /// <param name="url">URL to be inspected for a valid langtag prefix forming the first path part of the URL.</param>
        /// <param name="urlPatched">
        /// On success, set to the URL with the prefix path part removed.
        /// On failure, set to value of url param.
        /// </param>
        /// <returns>On success a LanguageTag instance, otherwise null.</returns>
        /// <remarks>
        /// <example>
        /// For URL /zh-Hans/account/signup we return "zh-Hans" and output /account/signup.
        /// </example>
        /// </remarks>
        public static LanguageTag UrlExtractLangTag(string url, out string urlPatched)
        {
           // Parse the url.
            System.Text.RegularExpressions.Match match = m_regex_parseUrl.Match(url);
           // If successful
            if (match.Success
                && match.Groups.Count == 2) {
               // Extract the langtag value.
                string langtag = match.Groups[1].Value;
               // Patch the url.
                urlPatched = url.Substring(langtag.Length +1);
                if (urlPatched.Length == 0) {
                    urlPatched = "/"; }
               // Success.
                return GetCachedInstance(langtag);
            }
           // No match.
            urlPatched = url;
            return null;
        }
        /// <summary>
        /// Patches in the langtag into the passed url, replacing any extant langtag in the url if necessary.
        /// </summary>
        /// <param name="url">
        /// URL to be patched.
        /// </param>
        /// <param name="langtag">
        /// Optional langtag to be patched into the URL, or null if any langtag 
        /// to be removed from the URL.
        /// </param>
        /// <returns>UriBuilder containing the modified version of url.</returns>
        /// <remarks>
        /// <para>"example.com/account/signup"         , "en" -> "example.com/en/account/signup"</para>
        /// <para>"example.com/zh-Hans/account/signup" , "en" -> "example.com/en/account/signup"</para>
        /// </remarks>
        public static UriBuilder UrlSetLangTag(string url, LanguageTag langtag)
        {
            UriBuilder ub = new UriBuilder(url);
            string urlPatched;
            UrlExtractLangTag(ub.Path, out urlPatched);
            ub.Path = urlPatched;
            if (langtag.IsValid()) {
                ub.PrependPath(langtag.ToString()); }
            return ub;
        }
    // Trace
        [Conditional("DEBUG")]
        public static void Trace_Match(string i_langtag_lhs, string i_langtag_rhs)
        {
            LanguageTag lhs = new LanguageTag(i_langtag_lhs);
            LanguageTag rhs = new LanguageTag(i_langtag_rhs);
            int score = lhs.Match(rhs);
            DebugHelpers.WriteLine("LanguageTag.Trace_Match -- Match({0}, {1}) = {2}", i_langtag_lhs, i_langtag_rhs, score.ToString());
        }
        [Conditional("DEBUG")]
        public static void Trace()
        {
            LanguageTag.Trace_Match("zh", "zh");
            LanguageTag.Trace_Match("zh", "zh-HK");
            LanguageTag.Trace_Match("zh", "zh-Hant");
            LanguageTag.Trace_Match("zh", "zh-Hant-HK");
            LanguageTag.Trace_Match("zh-HK", "zh");
            LanguageTag.Trace_Match("zh-HK", "zh-HK");
            LanguageTag.Trace_Match("zh-HK", "zh-Hant");
            LanguageTag.Trace_Match("zh-HK", "zh-Hant-HK");
            LanguageTag.Trace_Match("zh-Hant", "zh");
            LanguageTag.Trace_Match("zh-Hant", "zh-HK");
            LanguageTag.Trace_Match("zh-Hant", "zh-Hant");
            LanguageTag.Trace_Match("zh-Hant-HK", "zh-Hant-HK");
            LanguageTag.Trace_Match("zh-Hant-HK", "zh");
            LanguageTag.Trace_Match("zh-Hant-HK", "zh-HK");
            LanguageTag.Trace_Match("zh-Hant-HK", "zh-Hant");
            LanguageTag.Trace_Match("zh-Hant-HK", "zh-Hant-HK");

            LanguageTag.Trace_Match("dh", "zh");
            LanguageTag.Trace_Match("dh", "zh-HK");
            LanguageTag.Trace_Match("dh", "zh-Hant");
            LanguageTag.Trace_Match("dh", "zh-Hant-HK");
            LanguageTag.Trace_Match("dh-HK", "zh");
            LanguageTag.Trace_Match("dh-HK", "zh-HK");
            LanguageTag.Trace_Match("dh-HK", "zh-Hant");
            LanguageTag.Trace_Match("dh-HK", "zh-Hant-HK");
            LanguageTag.Trace_Match("dh-Hant", "zh");
            LanguageTag.Trace_Match("dh-Hant", "zh-HK");
            LanguageTag.Trace_Match("dh-Hant", "zh-Hant");
            LanguageTag.Trace_Match("dh-Hant-HK", "zh-Hant-HK");
            LanguageTag.Trace_Match("dh-Hant-HK", "zh");
            LanguageTag.Trace_Match("dh-Hant-HK", "zh-HK");
            LanguageTag.Trace_Match("dh-Hant-HK", "zh-Hant");
            LanguageTag.Trace_Match("dh-Hant-HK", "zh-Hant-HK");

            LanguageTag.Trace_Match("zh", "zh");
            LanguageTag.Trace_Match("zh", "zh-HK");
            LanguageTag.Trace_Match("zh", "zh-Hant");
            LanguageTag.Trace_Match("zh", "zh-Hant-HK");
            LanguageTag.Trace_Match("zh-IK", "zh");
            LanguageTag.Trace_Match("zh-IK", "zh-HK");
            LanguageTag.Trace_Match("zh-IK", "zh-Hant");
            LanguageTag.Trace_Match("zh-IK", "zh-Hant-HK");
            LanguageTag.Trace_Match("zh-Hant", "zh");
            LanguageTag.Trace_Match("zh-Hant", "zh-HK");
            LanguageTag.Trace_Match("zh-Hant", "zh-Hant");
            LanguageTag.Trace_Match("zh-Hant-IK", "zh-Hant-HK");
            LanguageTag.Trace_Match("zh-Hant-IK", "zh");
            LanguageTag.Trace_Match("zh-Hant-IK", "zh-HK");
            LanguageTag.Trace_Match("zh-Hant-IK", "zh-Hant");
            LanguageTag.Trace_Match("zh-Hant-IK", "zh-Hant-HK");

            LanguageTag.Trace_Match("zh", "zh");
            LanguageTag.Trace_Match("zh", "zh-HK");
            LanguageTag.Trace_Match("zh", "zh-Hant");
            LanguageTag.Trace_Match("zh", "zh-Hant-HK");
            LanguageTag.Trace_Match("zh-HK", "zh");
            LanguageTag.Trace_Match("zh-HK", "zh-HK");
            LanguageTag.Trace_Match("zh-HK", "zh-Hant");
            LanguageTag.Trace_Match("zh-HK", "zh-Hant-HK");
            LanguageTag.Trace_Match("zh-Iant", "zh");
            LanguageTag.Trace_Match("zh-Iant", "zh-HK");
            LanguageTag.Trace_Match("zh-Iant", "zh-Hant");
            LanguageTag.Trace_Match("zh-Iant-HK", "zh-Hant-HK");
            LanguageTag.Trace_Match("zh-Iant-HK", "zh");
            LanguageTag.Trace_Match("zh-Iant-HK", "zh-HK");
            LanguageTag.Trace_Match("zh-Iant-HK", "zh-Hant");
            LanguageTag.Trace_Match("zh-Iant-HK", "zh-Hant-HK");

            LanguageTag.Trace_Match("zh", "zh");
            LanguageTag.Trace_Match("zh", "zh-HK");
            LanguageTag.Trace_Match("zh", "zh-Hant");
            LanguageTag.Trace_Match("zh", "zh-Hant-HK");
            LanguageTag.Trace_Match("zh-IK", "zh");
            LanguageTag.Trace_Match("zh-IK", "zh-HK");
            LanguageTag.Trace_Match("zh-IK", "zh-Hant");
            LanguageTag.Trace_Match("zh-IK", "zh-Hant-HK");
            LanguageTag.Trace_Match("zh-Iant", "zh");
            LanguageTag.Trace_Match("zh-Iant", "zh-HK");
            LanguageTag.Trace_Match("zh-Iant", "zh-Hant");
            LanguageTag.Trace_Match("zh-Iant-HK", "zh-Hant-HK");
            LanguageTag.Trace_Match("zh-Iant-HK", "zh");
            LanguageTag.Trace_Match("zh-Iant-HK", "zh-HK");
            LanguageTag.Trace_Match("zh-Iant-HK", "zh-Hant");
            LanguageTag.Trace_Match("zh-Iant-HK", "zh-Hant-HK");
        }
    }


    public static partial class LanguageTagExtensions
    {
        public static bool IsValid(
            this ILanguageTag lt)
        {
            return lt != null && lt.GetLanguage() != null;
        }
    }

}
