using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using i18n;

namespace i18n.Tests
{
    /// <summary>
    /// Mock implementation of ITextLocalizer with simplest of logic:
    /// · Supports single app language: "en"
    /// · GetText checks that the UserLanguage spec. matches "en" and if so simply wraps 
    ///   the msgid as follows: "xxx<msgid>yyy". E.g. "Hello" -> "xxxHelloyyy".
    ///   If no language match then returns null.
    /// </summary>
    class TextLocalizer_Mock_PrefixSuffix : ITextLocalizer
    {
        readonly ConcurrentDictionary<string, LanguageTag> m_appLanguages = new ConcurrentDictionary<string,LanguageTag>();
        readonly string prefix;
        readonly string suffix;

        public void AddAppLanguage(string langtag)
        {
            m_appLanguages[langtag] = LanguageTag.GetCachedInstance(langtag);
        }

        public TextLocalizer_Mock_PrefixSuffix(
            string prefix = "",
            string suffix = "")
        {
            this.prefix = prefix;
            this.suffix = suffix;
            AddAppLanguage("en");
        }

	#region [ITextLocalizer]

        public virtual ConcurrentDictionary<string, LanguageTag> GetAppLanguages()
        {
            return m_appLanguages;
        }

        public virtual string GetText(string msgid, string msgcomment, LanguageItem[] languages, out LanguageTag o_langtag, int maxPasses = -1)
        {
            string s1;
            LanguageTag lt = LanguageMatching.MatchLists(
                languages,
                m_appLanguages.Values,
                msgid,
                null,
                out s1,
                maxPasses);
           //
            o_langtag = lt;
            if (lt.IsValid()) {
                return string.Format("{0}{1}{2}", prefix, msgid, suffix); }
            return null;
        }

    #endregion

    }
}
