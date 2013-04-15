using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n.Helpers
{
    public static class StringExtensions
    {
        /// <summary>
        /// String extension method to simplify testing for non-null/non-empty values.
        /// </summary>
        public static bool IsSet(
            this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Returns the line number (1-based) of the identified character in the string.
        /// </summary>
        public static int LineFromPos(this string S, int Pos)
        {
            int Res = 1;
            for (int i = 0; i < Pos; i++) {
                if (S[i] == '\n') {
                    Res++; }
            }
            return Res;
        }
    }
}
