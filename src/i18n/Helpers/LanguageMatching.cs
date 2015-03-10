using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n
{
    public static class LanguageMatching
    {
        /// <summary>
        /// Given a list of user-preferred languages (in order of precedence) and the list of languages
        /// in which an arbitrary resource is available (AppLanguages), returns the AppLanguage which
        /// the user is most likely able to understand.
        /// </summary>
        /// <param name="UserLanguages">
        /// A list of user-preferred languages (in order of precedence).
        /// </param>
        /// <param name="AppLanguages">
        /// The list of languages in which an arbitrary resource is available.
        /// </param>
        /// <param name="key">
        /// Optionally specifies the key or a message to be looked up in order to validate
        /// a language selection. Only if the language passes the validation will it be selected.
        /// Set in conjunction with TryGetTextFor.
        /// May be null (while TryGetTextFor is non-null) which specifies that one or more messages 
        /// must exists for a language for it to be considered valid (PO-valid).
        /// </param>
        /// <param name="TryGetTextFor">
        /// Optional delegate to be called in order to validate a language for selection.
        /// See TextLocalizer.TryGetTextFor for more details.
        /// </param>
        /// <param name="o_text">
        /// When language validation is enabled (TryGetTextFor is non-null) outputs the translated
        /// text that was returned by TryGetTextFor when the language was validated.
        /// If key == null then this will be set to "".
        /// </param>
        /// <param name="maxPasses">
        /// 0 - allow exact match only
        /// 1 - allow exact match or default-region match only
        /// 2 - allow exact match or default-region match or script match only
        /// 3 - allow exact match or default-region match or script match or language match only
        /// -1 to set to most tolerant (i.e. 4).
        /// </param>
        /// <param name="relatedTo">
        /// Optionally applies a filter to the user languages considered for a match.
        /// When set, then only user languages that have a matching language to that of relatedTo
        /// are considered.
        /// </param>
        /// <param name="palPrioritization">
        /// Indicates whether PAL Prioritization is enabled.
        /// </param>
        /// <returns>
        /// LanguageTag instance selected from AppLanguages with the best match, or null if there is no match
        /// at all (or UserLanguages and/or AppLanguages is empty).
        /// It is possible for there to be no match at all if no language subtag in the UserLanguages tags
        /// matches the same of any of the tags in AppLanguages list.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if UserLanguages or AppLanguages is null.</exception>
        /// <remarks>
        /// This method called many times per request. Every effort taken to avoid it making any heap allocations.<br/>
        /// <br/>
        /// Principle Application Language (PAL) Prioritization:<br/>
        ///   User has selected an explicit language in the webapp e.g. fr-CH (i.e. PAL is set to fr-CH).
        ///   Their browser is set to languages en-US, zh-Hans.
        ///   Therefore, UserLanguages[] equals fr-CH, en-US, zh-Hans.
        ///   We don't have a particular message in fr-CH, but have it in fr and fr-CA.
        ///   We also have message in en-US and zh-Hans.
        ///   We presume the message from fr or fr-CA is better match than en-US or zh-Hans.
        ///   However, without PAL prioritization, en-US is returned and failing that, zh-Hans.
        ///   Therefore, for the 1st entry in UserLanguages (i.e. explicit user selection in app)
        ///   we try all match grades first. Only if there is no match whatsoever for the PAL
        ///   do we move no to the other (browser) languages, where return to prioritizing match grade
        ///   i.e. loop through all the languages first at the strictest match grade before loosening 
        ///   to the next match grade, and so on.
        /// Refinement to PAL Prioritization:<br/>
        ///   UserLanguages (UL) = de-ch,de-at (PAL = de-ch)<br/>
        ///   AppLanguages  (AL) = de,de-at,en<br/>
        ///   There is no exact match for PAL in AppLanguages.<br/>
        ///   However:<br/>
        ///    1. the second UL (de-at) has an exact match with an AL<br/>
        ///    2. the parent of the PAL (de) has an exact match with an AL.<br/>
        ///   Normally, PAL Prioritization means that 2. takes precedence.
        ///   However, that means choosing de over de-at, when the user
        ///   has said they understand de-at (it being preferable to be
        ///   more specific, esp. in the case of different scripts under 
        ///   the same language).<br/>
        ///   Therefore, as a refinement to PAL Prioritization, before selecting
        ///   'de' we run the full algorithm again (without PAL Prioritization) 
        ///   but only considering langtags related to the PAL.
        /// </remarks>
        public static LanguageTag MatchLists(
            LanguageItem[] UserLanguages, 
            IEnumerable<LanguageTag> AppLanguages,
            string key,
            Func<string, string, string> TryGetTextFor,
            out string o_text,
            int maxPasses = -1,
            LanguageTag relatedTo = null,
            bool palPrioritization = true)
        {
            int idxUserLang = 0;
            LanguageTag ltUser;
           // Validate arguments.
            if (UserLanguages == null) { throw new ArgumentNullException("UserLanguages"); }
            if (AppLanguages == null) { throw new ArgumentNullException("AppLanguages"); }
            if (maxPasses > (int)LanguageTag.MatchGrade._MaxMatch) {
                maxPasses = (int)LanguageTag.MatchGrade._MaxMatch; }

            //#78
            //if (key != null && key.Equals("Sign In", StringComparison.InvariantCultureIgnoreCase)) {
            //    key = key; }

           // If one or more UserLanguages determined for the current request
            if (UserLanguages.Length != 0) {
               // First, find any match for the PAL (see PAL Prioritization notes above).
               // If a PAL has been determined for the request
                if (palPrioritization
                    && (ltUser = (LanguageTag)UserLanguages[0].LanguageTag) != null
                    && (relatedTo == null || ltUser.Match(relatedTo, LanguageTag.MatchGrade.LanguageMatch) != 0)) { // Apply any filter on eligible user languages.
                   // Wiz through all match grades for the Principle Application Language.
                    for (int pass = 0; pass <= (int)LanguageTag.MatchGrade._MaxMatch; ++pass) {
                        LanguageTag.MatchGrade matchGrade = (LanguageTag.MatchGrade)pass;
                        foreach (LanguageTag langApp in AppLanguages) {
                           // If languages do not match at the current grade...goto next.
                            if (ltUser.Match(langApp, matchGrade) == 0) {
                                continue; }
                           // Optionally test for a resource of the given key in the matching language.
                            if (TryGetTextFor != null) {
                                o_text = TryGetTextFor(langApp.ToString(), key);
                                if (o_text == null) {
                                    continue; }
                            }
                            else {
                                o_text = null; }
                           // We have a match between PAL and an AL that is NOT an exact match, 
                           // there may be a UL that is related to the PAL but has a closer (more specific) 
                           // match to an AL. See "Refinement to PAL Prioritization" notes above for more details.
                            if (matchGrade != LanguageTag.MatchGrade.ExactMatch) {

                                LanguageTag lt = MatchLists(
                                    UserLanguages, 
                                    AppLanguages,
                                    key,
                                    TryGetTextFor,
                                    out o_text,
                                    maxPasses,
                                    langApp,
                                    false); // false = disable PAL Prioritization.
                                if (lt != null) {
                                    return lt; }
                            }
                           // Match.
                            ++UserLanguages[idxUserLang].UseCount;
                            return langApp;
                        }
                    }
                }
               // PAL didn't match so skip over that now.
                ++idxUserLang;
               // No match for PAL, so now try for the browser languages, this time prioritizing the
               // match grade.
                for (int pass = 0; pass <= (int)LanguageTag.MatchGrade._MaxMatch; ++pass) {
                    LanguageTag.MatchGrade matchGrade = (LanguageTag.MatchGrade)pass;
                    for (int i = idxUserLang; i < UserLanguages.Length; ++i) {
                        ltUser = (LanguageTag)UserLanguages[i].LanguageTag;
                        if (ltUser == null) {
                            continue; }
                            // TODO: move the Match functionality to this class, and make it operate on ILanguageTag.
                            // Or consider making the Match logic more abstract, e.g. requesting number of passes from
                            // the object, and passing a pass value through to Match.
                       // Apply any filter on eligible user languages.
                        if (relatedTo != null) {
                            if (ltUser.Match(relatedTo, LanguageTag.MatchGrade.LanguageMatch) == 0) {
                                continue; }
                        }
                        foreach (LanguageTag langApp in AppLanguages) {
                           // If languages do not match at the current grade...goto next.
                            if (ltUser.Match(langApp, matchGrade) == 0) {
                                continue; }
                           // Optionally test for a resource of the given key in the matching language.
                            if (TryGetTextFor != null) {
                                o_text = TryGetTextFor(langApp.ToString(), key);
                                if (o_text == null) {
                                    continue; }
                            }
                            else {
                                o_text = null; }
                           // Match.
                            ++UserLanguages[i].UseCount;
                            return langApp;
                        }
                    }
                }
            }
           // No match at all.
            o_text = null;
            return null;
        }
    }
}
