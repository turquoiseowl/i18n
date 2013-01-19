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
        /// <param name="UserLanguages">A list of user-preferred languages (in order of preference).</param>
        /// <param name="AppLanguages">The list of languages in which an arbitrary resource is available.</param>
        /// <returns>
        /// LanguageTag instance selected from AppLanguages with the best match, or null if the is no match
        /// at all (or UserLanguages and/or AppLanguages is empty).
        /// It is possible for there to be no match at all if no language subtag in the UserLanguages tags
        /// matches the same of any of the tags in AppLanguages list.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if UserLanguages or AppLanguages is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if UserLanguages or AppLanguages is null.</exception>
        public static LanguageTag MatchLists(LanguageTag[] UserLanguages, LanguageTag[] AppLanguages)
        {
           // Validate arguments.
            if (UserLanguages == null) { throw new ArgumentNullException("UserLanguages"); }
            if (AppLanguages == null) { throw new ArgumentNullException("AppLanguages"); }
           //
            int pass = 1;
            foreach (LanguageTag langUser in UserLanguages) {
                foreach (LanguageTag langApp in AppLanguages) {
                    switch (pass) {
                        case 1: {
                            if (langUser.Match(langApp, LanguageTag.MatchGrade.ExactMatch) != 0) {
                                return langApp; }
                            break;
                        }
                        case 2: {
                            if (langUser.Match(langApp, LanguageTag.MatchGrade.ScriptMatch) != 0) {
                                return langApp; }
                            break;
                        }
                        case 3: {
                            if (langUser.Match(langApp, LanguageTag.MatchGrade.LanguageMatch) != 0) {
                                return langApp; }
                            break;
                        }
                    }
                }
               // Try next phase or get out if done.
                if (++pass == 4) {
                    break; }
            }
           // No match at all.
            return null;
        }
    }
}
