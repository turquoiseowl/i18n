using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace i18n
{
    /// <summary>
    /// Helper class for interpreting the value of and HTTP Accept-Language header,
    /// both in terms of the language tags and the relative preference for such.
    /// </summary>
    public class AcceptLanguage
    {
    // Helpers
        public class myReverserClass : IComparer<float>
        {
            int IComparer<float>.Compare(float x, float y)
            {
                return
                    x < y ? 1:
                    x > y ? -1:
                    0;
            }
        }
        private static readonly myReverserClass s_myReverserClass = new myReverserClass();
    // Props
        /// <summary>
        /// Ordered collection of collections of langtags, keyed and ordered by the qvalue for the langtags
        /// in the inner collection. Langtags of qvalue=0 are included and should be
        /// interpreted as DO NOT RETURN THIS LANGUAGE.
        /// See ToString impl. for correct way to enumerate the langtags in order of preference.
        /// </summary>
        public SortedList<float, List<LanguageTag> > Languages { get; private set; }
    // Con
        /// <summary>
        /// Parses an HTTP Accept-Language header value.
        /// E.g. "de;q=0.5, en;q=1 ,  fr-FR;q=0,ga;q=0.5".
        /// Notably, is able to re-order elements based on quality.
        /// </summary>
        /// <param name="headerval">
        /// HTTP Accept-Language header value.
        /// http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html.
        /// </param>
        public AcceptLanguage(string headerval)
        {
        // This method is designed to be as efficient as possible (avoiding string allocations where possible).
        //
            if (null == headerval) {
                throw new ArgumentNullException("headerval"); }
            int begin, end, pos1;
            int len = headerval.Length;
           // Init
            var languages = new SortedList<float, List<LanguageTag>>(s_myReverserClass);
           // For each language component of the header (delimited by comma)
            for (begin = 0; begin < len; begin = end +1) {
                end = headerval.IndexOf(',', begin);
                if (-1 == end) {
                    end = len; }
                string langsubtag;
                float qvalue = 1;
                pos1 = headerval.IndexOf(';', begin);
                if (-1 != pos1) {
                   // pos1 -> ";q=n"
                    if (pos1 -begin < 2 // room for valid langsubtag
                        || pos1 + 3 >= headerval.Length
                        || headerval[pos1 + 1] != 'q'
                        || headerval[pos1 + 2] != '=') {
                        continue; }
                    if (!ParseHelpers.TryParseDecimal(headerval, pos1 + 3, -1, out qvalue)) {
                        continue; }
                    if (qvalue < 0f || qvalue > 1.0f) {
                        continue; }
                }
                else {
                    pos1 = end; }
               // Skip over any whitespace, thus hopefully deeming the following Trim redundant.
                while (headerval[begin] == ' ') ++begin;
               // Extract language subtag e.g. "fr".
                langsubtag = headerval.Substring(begin, pos1 -begin).Trim();
               //
                LanguageTag lt = LanguageTag.GetCachedInstance(langsubtag);
                if (!lt.Language.IsSet()) {
                    continue; }
               //
                List<LanguageTag> lts;
                if (!languages.ContainsKey(qvalue)) {
                    languages.Add(qvalue, lts = new List<LanguageTag>()); }
                else {
                    lts = languages[qvalue]; }
                lts.Add(lt);
            }
           // Done.
            Languages = languages;
        }
    // [Object]
        public override string ToString()
        {
            string str = "";
            foreach (var element in Languages) {
                foreach (var langtag in element.Value) {
                    if (str.IsSet()) {
                        str += ","; }
                    str += langtag + ";q=" + element.Key;
                }
            }
            return str;
        }
    }
}
