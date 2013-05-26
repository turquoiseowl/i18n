using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace i18n
{
    internal static class Extensions
    {

        /// <summary>
        /// Helper for testing whether a string ends with one of any out of a collection of strings,
        /// in a case-insentive way.
        /// </summary>
        /// <param name="lhs">LHS string for the comparison.</param>
        /// <param name="rhs">Zero or more RHS strings for the comparison.</param>
        /// <returns>The first RHS string that matches the end of LHS, or null if none match.</returns>
        public static string EndsWithAnyIgnoreCase(this string lhs, params string[] rhs)
        {
            return rhs.FirstOrDefault(str => lhs.EndsWith(str, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Isolates and returns the character sequence between any first and last quote chars.
        /// </summary>
        /// <param name="lhs">Subject string possibly containing a quoted sequence.</param>
        /// <param name="quotechar">Quote char, defaults to double quotes. May be a string of more than one character.</param>
        /// <returns>
        /// Any character sequence contained within the first and last occurence of quotechar.
        /// Empty string if the first and last occurrence of quotechar are adjacent chars.
        /// Null if no welformed quoted sequence found.
        /// </returns>
        public static string Unquote(this string lhs, string quotechar = "\"")
        {
            int begin = lhs.IndexOf(quotechar, StringComparison.Ordinal);
            if (begin == -1) {
                return null; }
            int end = lhs.LastIndexOf(quotechar, StringComparison.Ordinal);
            if (end <= begin) {
                return null; }
            return lhs.Substring(begin +1, end -begin -1);
        }

        /// <summary>
        /// Looks up in the subject string standard C escape sequences and converts them
        /// to their actual character counterparts.
        /// </summary>
        /// <seealso>
        ///   <cref>http://stackoverflow.com/questions/6629020/evaluate-escaped-string/8854626#8854626</cref>
        /// </seealso>
        public static string Unescape(this string s)
        {
            var sb = new StringBuilder();
            var r = new Regex("\\\\[abfnrtv?\"'\\\\]|\\\\[0-3]?[0-7]{1,2}|\\\\u[0-9a-fA-F]{4}|.");
            MatchCollection mc = r.Matches(s, 0);

            foreach (Match m in mc) {
                if (m.Length == 1) {
                    sb.Append(m.Value);
                } else {
                    if (m.Value[1] >= '0' && m.Value[1] <= '7') {
                        int i = 0;

                        for (int j = 1; j < m.Length; j++) {
                            i *= 8;
                            i += m.Value[j] - '0';
                        }

                        sb.Append((char)i);
                    } else if (m.Value[1] == 'u') {
                        int i = 0;

                        for (int j = 2; j < m.Length; j++) {
                            i *= 16;

                            if (m.Value[j] >= '0' && m.Value[j] <= '9') {
                                i += m.Value[j] - '0';
                            } else if (m.Value[j] >= 'A' && m.Value[j] <= 'F') {
                                i += m.Value[j] - 'A' + 10;
                            } else if (m.Value[j] >= 'a' && m.Value[j] <= 'a') {
                                i += m.Value[j] - 'a' + 10;
                            }
                        }

                        sb.Append((char)i);
                    } else {
                        switch (m.Value[1]) {
                            case 'a':
                                sb.Append('\a');
                                break;
                            case 'b':
                                sb.Append('\b');
                                break;
                            case 'f':
                                sb.Append('\f');
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 'r':
                                sb.Append('\r');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                            case 'v':
                                sb.Append('\v');
                                break;
                            default:
                                sb.Append(m.Value[1]);
                                break;
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}
