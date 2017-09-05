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
    /// · Supports single msgid/msgstr pair passed to cstor.
    /// · GetText checks that the UserLanguage spec. matches "en" and if so
    ///   and msgid matches that passed to cstor, then returns the msgstr passed to cstor.
    ///   Otherwise returns null.
    /// </summary>
    class TextLocalizer_Mock_Generic : ITextLocalizer
    {
        readonly ConcurrentDictionary<string, LanguageTag> m_appLanguages = new ConcurrentDictionary<string,LanguageTag>();
        readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> m_messages = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

        public void AddAppLanguage(string langtag)
        {
            if (!m_appLanguages.ContainsKey(langtag))
            {
                m_appLanguages[langtag] = LanguageTag.GetCachedInstance(langtag);
                m_messages[langtag] = new ConcurrentDictionary<string, string>();
            }
        }

        public TextLocalizer_Mock_Generic()
        {
        }

        public void AddMessage(string langtag, string msgid, string msgstr)
        {
            AddAppLanguage(langtag);
            m_messages[langtag][msgid] = msgstr;
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
            if (!lt.IsValid()) {
                return null; }
            if (!m_messages.ContainsKey(o_langtag.Language) || !m_messages[o_langtag.Language].ContainsKey(msgid))
                return null;
            return
                m_messages[o_langtag.Language][msgid];
        }

    #endregion

    }
}
