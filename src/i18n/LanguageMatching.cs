using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n
{
    public static class LanguageMatching
    {
        /// <summary>
        /// Given a list of user-preferred languages (in order of preference) and the list of languages
        /// in which an arbitrary resource is available (AppLanguages), returns the AppLanguage which
        /// the user is most likely able to understand.
        /// </summary>
        /// <param name="UserLanguages">
        /// A list of user-preferred languages (in order of preference).
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
        /// <returns>
        /// LanguageTag instance selected from AppLanguages with the best match, or null if there is no match
        /// at all (or UserLanguages and/or AppLanguages is empty).
        /// It is possible for there to be no match at all if no language subtag in the UserLanguages tags
        /// matches the same of any of the tags in AppLanguages list.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if UserLanguages or AppLanguages is null.</exception>
        public static LanguageTag MatchLists(
            LanguageItem[] UserLanguages, 
            IEnumerable<KeyValuePair<string, LanguageTag> > AppLanguages,
            string key,
            Func<string, string, string> TryGetTextFor,
            out string o_text,
            int maxPasses = -1)
        {
        // This method called many times per request. Every effort taken to avoid it making any heap allocations.
        //
        // Principle Application Language (PAL) Prioritization:
        //   User has selected an explicit language in the webapp e.g. fr-CH (i.e. PAL is set to fr-CH).
        //   Their browser is set to languages en-US, en, zh-Hans.
        //   Therefore, UserLanguages[] equals fr-CH, en-US, zh-Hans.
        //   We don't have a particular message in fr-CH, but have it in fr and fr-CA.
        //   We also have message in en-US and zh-Hans.
        //   Surely, the message from fr or fr-CA is better match than en-US or zh-Hans.
        //   However, without PAL prioritization, en-US is returned and failing that, zh-Hans.
        //   Therefore, for the 1st entry in UserLanguages (i.e. explicit user selection in app)
        //   we try all match grades first. Only if there is not match whatsoever for the PAL
        //   so we move no to the other (browser) languages, where return to prioritizing match grade
        //   i.e. loop through all the languages first at the strictest match grade before loosening 
        //   to the next match grade, and so on.
        //
            int idxUserLang = 0;
           // Validate arguments.
            if (UserLanguages == null) { throw new ArgumentNullException("UserLanguages"); }
            if (AppLanguages == null) { throw new ArgumentNullException("AppLanguages"); }
            if (maxPasses > (int)LanguageTag.MatchGrade._MaxMatch) {
                maxPasses = (int)LanguageTag.MatchGrade._MaxMatch; }

            //MC002
            //if (key == "Sign In") {
            //    key = key; }

            if (UserLanguages.Length != 0) {
               // First, find any match for the PAL (if set) (see PAL Prioritization notes above).
               // Wiz through all match grades for any Principle Application Language.
                for (int pass = 0; pass <= (int)LanguageTag.MatchGrade._MaxMatch; ++pass) {
                    LanguageTag.MatchGrade matchGrade = (LanguageTag.MatchGrade)pass;
                    LanguageTag ltUser = (LanguageTag)UserLanguages[idxUserLang].LanguageTag;
                    if (ltUser == null) {
                        continue; }
                        // TODO: move the Match functionality to this class, and make it operate on ILanguageTag.
                        // Or consider making the Match logic more abstract, e.g. requesting number of passes from
                        // the object, and passing a pass value through to Match.
                    foreach (KeyValuePair<string, LanguageTag> langApp in AppLanguages) {
                       // If languages do not match at the current grade...goto next.
                        if (ltUser.Match(langApp.Value, matchGrade) == 0) {
                            continue; }
                       // Optionally test for a resource of the given key in the matching language.
                        if (TryGetTextFor != null) {
                            o_text = TryGetTextFor(langApp.Key, key);
                            if (o_text == null) {
                                continue; }
                        }
                        else {
                            o_text = null; }
                       // Match.
                        ++UserLanguages[idxUserLang].UseCount;
                        return langApp.Value;
                    }
                }
                ++idxUserLang;
               // No match for PAL, so now try for the browser languages, this time prioritizing the
               // match grade.
                for (int pass = 0; pass <= (int)LanguageTag.MatchGrade._MaxMatch; ++pass) {
                    LanguageTag.MatchGrade matchGrade = (LanguageTag.MatchGrade)pass;
                    for (; idxUserLang < UserLanguages.Length; ++idxUserLang) {
                        LanguageTag ltUser = (LanguageTag)UserLanguages[idxUserLang].LanguageTag;
                        if (ltUser == null) {
                            continue; }
                            // TODO: move the Match functionality to this class, and make it operate on ILanguageTag.
                            // Or consider making the Match logic more abstract, e.g. requesting number of passes from
                            // the object, and passing a pass value through to Match.
                        foreach (KeyValuePair<string, LanguageTag> langApp in AppLanguages) {
                           // If languages do not match at the current grade...goto next.
                            if (ltUser.Match(langApp.Value, matchGrade) == 0) {
                                continue; }
                           // Optionally test for a resource of the given key in the matching language.
                            if (TryGetTextFor != null) {
                                o_text = TryGetTextFor(langApp.Key, key);
                                if (o_text == null) {
                                    continue; }
                            }
                            else {
                                o_text = null; }
                           // Match.
                            ++UserLanguages[idxUserLang].UseCount;
                            return langApp.Value;
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
