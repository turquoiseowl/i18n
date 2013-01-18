using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
    /// <seealso cref="http://www.microsoft.com/resources/msdn/goglobal/default.mspx"/>
    public class LanguageTag
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
            /// </summary>
            ExactMatch,
            /// <summary>
            /// Only consider a match where language and script parts match. Region part need not match.
            /// </summary>
            ScriptMatch,
            /// <summary>
            /// Only consider a match where language matches. Script and region parts need not match.
            /// </summary>
            LanguageMatch,
        }
    // Data
        static readonly Regex m_regex = new Regex(
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
        private static ConcurrentDictionary<string, LanguageTag> s_cache = new ConcurrentDictionary<string, LanguageTag>();
            // Facilitates fast and efficient re-use of languag tag instances.
            // Key = langtag string.
            // Write-access to this member to be serialized via s_sync.
        private static readonly object s_sync = new object();
            // Facilitates serialization of write-access to s_cache.
    // Props
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
    // Con
        /// <summary>
        /// Constructs a new instance based on a langugae tag string.
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
        /// <seealso cref="http://www.microsoft.com/resources/msdn/goglobal/default.mspx"/>
        public LanguageTag(string langtag)
        {
            langtag = langtag.Trim();
           // Normalize certain langtags:
           // «LX113» http://www.w3.org/International/articles/language-tags/#script
            for (int i = 0; i < NormalizedLangTags.GetLength(0); ++i) {
                if (0 == string.Compare(langtag, NormalizedLangTags[i,0], true)) {
                    langtag = NormalizedLangTags[i,1];
                    break;
                }
            }
           // Parse the langtag.
            Match match = m_regex.Match(langtag);
            if (match.Success
                && match.Groups.Count == 4) {
                Language = match.Groups[1].Value;
                Script   = match.Groups[2].Value;
                Region   = match.Groups[3].Value;
            }
            Debug.Assert(ToString() == langtag);
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
        /// <returns>Either new or pre-exisiting instance.</returns>
        /// <seealso cref="http://www.microsoft.com/resources/msdn/goglobal/default.mspx"/>
        public static LanguageTag GetCachedInstance(string langtag)
        {
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
               // Created new instance.
                s_cache[langtag] = result = new LanguageTag(langtag);
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
            StringBuilder sb = new StringBuilder(Language);
            if (Script.IsSet()) {
                sb.Append("-").Append(Script); }
            if (Region.IsSet()) {
                sb.Append("-").Append(Region); }
            return sb.ToString();
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
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/windows/apps/jj673578.aspx"/>
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
           // F.
            return 0;
        }
        /// <summary>
        /// Looks up in the passed collection of supported AppLanguages the language that is best matched
        /// to this langtag. I.e. the written AppLanguage that a user understanding this langtag
        /// will most-likely understand.
        /// </summary>
        /// <param name="i_langtag">Language tag to match.</param>
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
    // Test
        [Conditional("DEBUG")]
        public static void TraceMatch(string i_langtag_lhs, string i_langtag_rhs)
        {
            LanguageTag lhs = new LanguageTag(i_langtag_lhs);
            LanguageTag rhs = new LanguageTag(i_langtag_rhs);
            int score = lhs.Match(rhs);
            //DebugHelpers.WriteLine("LanguageTag.TraceMatch -- Match({0}, {1}) = {2}", i_langtag_lhs, i_langtag_rhs, score.ToString());
        }
        [Conditional("DEBUG")]
        public static void Test()
        {
            LanguageTag.TraceMatch("zh", "zh");
            LanguageTag.TraceMatch("zh", "zh-HK");
            LanguageTag.TraceMatch("zh", "zh-Hant");
            LanguageTag.TraceMatch("zh", "zh-Hant-HK");
            LanguageTag.TraceMatch("zh-HK", "zh");
            LanguageTag.TraceMatch("zh-HK", "zh-HK");
            LanguageTag.TraceMatch("zh-HK", "zh-Hant");
            LanguageTag.TraceMatch("zh-HK", "zh-Hant-HK");
            LanguageTag.TraceMatch("zh-Hant", "zh");
            LanguageTag.TraceMatch("zh-Hant", "zh-HK");
            LanguageTag.TraceMatch("zh-Hant", "zh-Hant");
            LanguageTag.TraceMatch("zh-Hant-HK", "zh-Hant-HK");
            LanguageTag.TraceMatch("zh-Hant-HK", "zh");
            LanguageTag.TraceMatch("zh-Hant-HK", "zh-HK");
            LanguageTag.TraceMatch("zh-Hant-HK", "zh-Hant");
            LanguageTag.TraceMatch("zh-Hant-HK", "zh-Hant-HK");

            LanguageTag.TraceMatch("dh", "zh");
            LanguageTag.TraceMatch("dh", "zh-HK");
            LanguageTag.TraceMatch("dh", "zh-Hant");
            LanguageTag.TraceMatch("dh", "zh-Hant-HK");
            LanguageTag.TraceMatch("dh-HK", "zh");
            LanguageTag.TraceMatch("dh-HK", "zh-HK");
            LanguageTag.TraceMatch("dh-HK", "zh-Hant");
            LanguageTag.TraceMatch("dh-HK", "zh-Hant-HK");
            LanguageTag.TraceMatch("dh-Hant", "zh");
            LanguageTag.TraceMatch("dh-Hant", "zh-HK");
            LanguageTag.TraceMatch("dh-Hant", "zh-Hant");
            LanguageTag.TraceMatch("dh-Hant-HK", "zh-Hant-HK");
            LanguageTag.TraceMatch("dh-Hant-HK", "zh");
            LanguageTag.TraceMatch("dh-Hant-HK", "zh-HK");
            LanguageTag.TraceMatch("dh-Hant-HK", "zh-Hant");
            LanguageTag.TraceMatch("dh-Hant-HK", "zh-Hant-HK");

            LanguageTag.TraceMatch("zh", "zh");
            LanguageTag.TraceMatch("zh", "zh-HK");
            LanguageTag.TraceMatch("zh", "zh-Hant");
            LanguageTag.TraceMatch("zh", "zh-Hant-HK");
            LanguageTag.TraceMatch("zh-IK", "zh");
            LanguageTag.TraceMatch("zh-IK", "zh-HK");
            LanguageTag.TraceMatch("zh-IK", "zh-Hant");
            LanguageTag.TraceMatch("zh-IK", "zh-Hant-HK");
            LanguageTag.TraceMatch("zh-Hant", "zh");
            LanguageTag.TraceMatch("zh-Hant", "zh-HK");
            LanguageTag.TraceMatch("zh-Hant", "zh-Hant");
            LanguageTag.TraceMatch("zh-Hant-IK", "zh-Hant-HK");
            LanguageTag.TraceMatch("zh-Hant-IK", "zh");
            LanguageTag.TraceMatch("zh-Hant-IK", "zh-HK");
            LanguageTag.TraceMatch("zh-Hant-IK", "zh-Hant");
            LanguageTag.TraceMatch("zh-Hant-IK", "zh-Hant-HK");

            LanguageTag.TraceMatch("zh", "zh");
            LanguageTag.TraceMatch("zh", "zh-HK");
            LanguageTag.TraceMatch("zh", "zh-Hant");
            LanguageTag.TraceMatch("zh", "zh-Hant-HK");
            LanguageTag.TraceMatch("zh-HK", "zh");
            LanguageTag.TraceMatch("zh-HK", "zh-HK");
            LanguageTag.TraceMatch("zh-HK", "zh-Hant");
            LanguageTag.TraceMatch("zh-HK", "zh-Hant-HK");
            LanguageTag.TraceMatch("zh-Iant", "zh");
            LanguageTag.TraceMatch("zh-Iant", "zh-HK");
            LanguageTag.TraceMatch("zh-Iant", "zh-Hant");
            LanguageTag.TraceMatch("zh-Iant-HK", "zh-Hant-HK");
            LanguageTag.TraceMatch("zh-Iant-HK", "zh");
            LanguageTag.TraceMatch("zh-Iant-HK", "zh-HK");
            LanguageTag.TraceMatch("zh-Iant-HK", "zh-Hant");
            LanguageTag.TraceMatch("zh-Iant-HK", "zh-Hant-HK");

            LanguageTag.TraceMatch("zh", "zh");
            LanguageTag.TraceMatch("zh", "zh-HK");
            LanguageTag.TraceMatch("zh", "zh-Hant");
            LanguageTag.TraceMatch("zh", "zh-Hant-HK");
            LanguageTag.TraceMatch("zh-IK", "zh");
            LanguageTag.TraceMatch("zh-IK", "zh-HK");
            LanguageTag.TraceMatch("zh-IK", "zh-Hant");
            LanguageTag.TraceMatch("zh-IK", "zh-Hant-HK");
            LanguageTag.TraceMatch("zh-Iant", "zh");
            LanguageTag.TraceMatch("zh-Iant", "zh-HK");
            LanguageTag.TraceMatch("zh-Iant", "zh-Hant");
            LanguageTag.TraceMatch("zh-Iant-HK", "zh-Hant-HK");
            LanguageTag.TraceMatch("zh-Iant-HK", "zh");
            LanguageTag.TraceMatch("zh-Iant-HK", "zh-HK");
            LanguageTag.TraceMatch("zh-Iant-HK", "zh-Hant");
            LanguageTag.TraceMatch("zh-Iant-HK", "zh-Hant-HK");
        }
    }
}
