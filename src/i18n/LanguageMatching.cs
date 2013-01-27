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
        /// a language selection. Only is the language passes the validation will it be selected.
        /// Set in conjunction with TryGetTextFor.
        /// May be null (while TryGetTextFor is non-null) which specifies that one or more messages 
        /// must exists for a language for it to be considered valid (PO-valid).
        /// </param>
        /// <param name="TryGetTextFor">
        /// Optional delegate to be called in order to validate a language for selection.
        /// See LocalizingService.TryGetTextFor for more details.
        /// </param>
        /// <param name="o_text">
        /// When language validation is enabled (TryGetTextFor is non-null) outputs the translated
        /// text that was returned by TryGetTextFor when teh language was validated.
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
        /// LanguageTag instance selected from AppLanguages with the best match, or null if the is no match
        /// at all (or UserLanguages and/or AppLanguages is empty).
        /// It is possible for there to be no match at all if no language subtag in the UserLanguages tags
        /// matches the same of any of the tags in AppLanguages list.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if UserLanguages or AppLanguages is null.</exception>
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
           // Validate arguments.
            if (UserLanguages == null) { throw new ArgumentNullException("UserLanguages"); }
            if (AppLanguages == null) { throw new ArgumentNullException("AppLanguages"); }
            if (maxPasses > (int)LanguageTag.MatchGrade._MaxMatch) {
                maxPasses = (int)LanguageTag.MatchGrade._MaxMatch; }
           //
            for (int pass = 0; pass <= (int)LanguageTag.MatchGrade._MaxMatch; ++pass) {
                LanguageTag.MatchGrade matchGrade = (LanguageTag.MatchGrade)pass;
                for (int i = 0; i < UserLanguages.Length; ++i) {
                    LanguageTag ltUser = (LanguageTag)UserLanguages[i].LanguageTag;
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
                        ++UserLanguages[i].UseCount;

                        if (UserLanguages.Length > 2) {
                            int a = 10;
                        }

                        return langApp.Value;
                    }
                }
            }
           // No match at all.
            o_text = null;
            return null;
        }
    }
}
