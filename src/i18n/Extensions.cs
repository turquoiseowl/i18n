using System;
using System.Linq;

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
            foreach (string str in rhs) {
                if (lhs.EndsWith(str, StringComparison.OrdinalIgnoreCase)) {
                    return str; }
            }
            return null;
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
            int begin = lhs.IndexOf(quotechar);
            if (begin == -1) {
                return null; }
            int end = lhs.LastIndexOf(quotechar);
            if (end <= begin) {
                return null; }
            return lhs.Substring(begin +1, end -begin -1);
        }
    }
}
