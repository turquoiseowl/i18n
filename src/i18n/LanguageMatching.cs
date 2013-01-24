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
            IEnumerable<LanguageTag> AppLanguages,
            string key,
            Func<string, string, string> TryGetTextFor,
            out string o_text)
        {
        // This method called many times per request. Every effort taken to avoid it making any heap allocations.
        //
           // Validate arguments.
            if (UserLanguages == null) { throw new ArgumentNullException("UserLanguages"); }
            if (AppLanguages == null) { throw new ArgumentNullException("AppLanguages"); }
           //
            for (int pass = 1; pass < 4; ++pass) {
                LanguageTag.MatchGrade matchGrade;
                switch (pass) {
                    case 1: matchGrade = LanguageTag.MatchGrade.ExactMatch; break;
                    case 2: matchGrade = LanguageTag.MatchGrade.ScriptMatch; break;
                    case 3: matchGrade = LanguageTag.MatchGrade.LanguageMatch; break;
                    default: throw new InvalidOperationException();
                }
                foreach (LanguageItem langUser in UserLanguages) {
                    LanguageTag ltUser = (LanguageTag)langUser.LanguageTag;
                        // TODO: move the Match functionality to this class, and make it operate on ILanguageTag.
                        // Or consider making the Match logic more abstract, e.g. requesting number of passed from
                        // the object, and passing a pass value through to Match.
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
                        return langApp;
                    }
                }
            }
           // No match at all.
            o_text = null;
            return null;
        }
    }
}
