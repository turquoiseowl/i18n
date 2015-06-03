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
    class TextLocalizer_Mock_SingleMessage : ITextLocalizer
    {
        readonly ConcurrentDictionary<string, LanguageTag> m_appLanguages = new ConcurrentDictionary<string,LanguageTag>();
        readonly string msgid;
        readonly string msgstr;

        public void AddAppLanguage(string langtag)
        {
            m_appLanguages[langtag] = LanguageTag.GetCachedInstance(langtag);
        }

        public TextLocalizer_Mock_SingleMessage(
            string msgid,
            string msgstr)
        {
            this.msgid = msgid;
            this.msgstr = msgstr;
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
            if (!lt.IsValid()) {
                return null; }
            if (this.msgid != msgid) {
                return null; }
            return msgstr;
        }

    #endregion

    }
}
