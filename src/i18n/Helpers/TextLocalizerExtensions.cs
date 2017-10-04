namespace i18n.Helpers
{
    public static class TextLocalizerExtensions
    {
        /// <summary>
        /// Wrapper around ITextLocalizer.GetText which is more resilient where the msgid
        /// passed may have been HtmlDecoded. Ref Issue #105 and #202.
        /// </summary>
        /// <param name="textLocalizer">Interface on text localizer object.</param>
        /// <param name="allowLookupWithHtmlDecodedMsgId">
        /// Controls whether a lookup will be attempted with HtmlDecoded-msgid should the first lookup with raw msgid fail.
        /// </param>
        /// <param name="msgid"><see cref="ITextLocalizer.GetText"/></param>
        /// <param name="msgcomment"><see cref="ITextLocalizer.GetText"/></param>
        /// <param name="languages"><see cref="ITextLocalizer.GetText"/></param>
        /// <param name="o_langtag"><see cref="ITextLocalizer.GetText"/></param>
        /// <param name="maxPasses"><see cref="ITextLocalizer.GetText"/></param>
        /// <returns><see cref="ITextLocalizer.GetText"/></returns>
        public static string GetText(
            this ITextLocalizer textLocalizer,
            bool allowLookupWithHtmlDecodedMsgId,
            string msgid, 
            string msgcomment, 
            LanguageItem[] languages, 
            out LanguageTag o_langtag,
            int maxPasses = -1)
        {
        // Lookup resource using canonical msgid with optional attempt with HtmlDecode of msgid.
        // 1. Try lookup with raw msgid as is.
        // 2. Failing that, if allowed, try lookup with HtmlDecoded msgid (ref Issue #105 and #202).
        // 3. Failing that, return the raw msgid.
        // See also unit test: NuggetLocalizer_can_process_nugget_htmlencoded.
        //
            string message;
           // 1.
            message = textLocalizer.GetText(
                msgid,
                msgcomment,
                languages,
                out o_langtag,
                maxPasses);
            if (message != null
                && message != msgid) { // <- message != msgid indicates message was found, and vice versa.
                return message; }
           // 2.
            if (allowLookupWithHtmlDecodedMsgId) {
                string msgIdHtmlDecoded = System.Web.HttpUtility.HtmlDecode(msgid);
                message = textLocalizer.GetText(
                    msgIdHtmlDecoded, 
                    System.Web.HttpUtility.HtmlDecode(msgcomment), 
                    languages,
                    out o_langtag,
                    maxPasses);
                if (message != null
                    && message != msgIdHtmlDecoded) { // <- message != msgIdHtmlDecoded indicates message was found, and vice versa.
                    return message; }
            }
           // 3.
            return msgid;
        }
    }
}
