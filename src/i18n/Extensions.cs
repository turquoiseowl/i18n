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
    }
}
