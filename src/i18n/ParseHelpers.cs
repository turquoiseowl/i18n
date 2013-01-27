using System;
using System.Diagnostics;
using System.Text;

namespace i18n
{
    internal static class ParseHelpers
    {
        /// <summary>
        /// Efficient (allocation-free) parsing of numerical strings with support for substrings
        /// and number extraction. Non-numerical characters are skipped until and parsing started
        /// from the first numeral encountered and ends either at the end of the substring or the next
        /// non-numeral char, whichever comes first.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="begin">Zero-based index of the first char in the string from which to start parsing.</param>
        /// <param name="end">Zero-based index +1 of the last char in the string to parse, or -1 parse to the end of string.</param>
        /// <param name="result">On success outputs the parsed value.</param>
        /// <returns>Indication of success.</returns>
        public static bool TryParseDecimal(string s, int begin, int end, out double result, char decimalpoint = '.')
        {
            int fracDigits = _TryParseDecimal(s, begin, end, out result, decimalpoint);
            if (-1 == fracDigits) {
                return false; }
            return true;
        }
        public static bool TryParseDecimal(string s, int begin, int end, out float result, char decimalpoint = '.')
        {
            double n;
            int fracDigits = _TryParseDecimal(s, begin, end, out n, decimalpoint);
            if (-1 == fracDigits) {
                result = 0;
                return false; }
            result = (float)Math.Round(n, fracDigits); // Round off any noise.
            return true;
        }
        
        /// <summary>
        /// Helper for parsing decimals.
        /// </summary>
        /// <returns>Number of fractional digits if successfule; owise -1.</returns>
        private static int _TryParseDecimal(string s, int begin, int end, out double result, char decimalpoint)
        {
            try {
                if (-1 == end) {
                    end = s.Length; }
                if (end <= begin) {
                    result = 0;
                    return -1; }
                char c;
                double r = 0;
                int sign = 1;
                bool fParseStarted = false;
                int idxDecimalPoint = -1;
                int digits = 0;
                for (int pos = begin; pos < end; ++pos) {
                    c = s[pos];
                   // If non-digit char
                    if ((c >= '0' && c <= '9')) {
                       // Start parsing the number.
                        if (!fParseStarted) {
                            fParseStarted = true; }
                        r = (r * 10) + (c - '0');
                        ++digits;
                    }
                   // If first decimal point
                    else if (c == decimalpoint
                        && -1 == idxDecimalPoint) {
                        idxDecimalPoint = digits;
                    }
                   // If sign
                    else if (c == '-'
                        && !fParseStarted) {
                        sign = -1;
                    }
                    else {
                        if (fParseStarted) {
                            break; }
                       // Decimal point char or sign where not sequential with digits, so reset.
                        idxDecimalPoint = -1;
                        sign = 1;
                    }
                }
               // If no digits encountered...fail.
                if (0 == digits) {
                    result = 0;
                    return -1; }
               // At this point we may have the following:
               //
               //   For input: 1.23456
               //   r = 123456
               //   posFirstDigit = 0
               //   digits = 6;
               //   idxDecimalPoint = 1
               //
               //   For input: 123.456
               //   r = 123456
               //   digits = 6;
               //   idxDecimalPoint = 3
               //
               //   For input: .456
               //   r = 456
               //   digits = 3;
               //   idxDecimalPoint = 0
               //
               //   For input: 123.
               //   r = 123
               //   digits = 3;
               //   idxDecimalPoint = 3
               //
               //   For input: 123.0
               //   r = 1230
               //   digits = 4;
               //   idxDecimalPoint = 3
               //
               // Therefore we need to divide r by 10^(digits -idxDecimalPoint).
                int fracDigits = 0;
                if (-1 != idxDecimalPoint
                    && digits > idxDecimalPoint) {
                    fracDigits = digits -idxDecimalPoint;
                    r = r / Math.Pow(10, fracDigits);
                    r = Math.Round(r, fracDigits); // Round off any noise.
                }
               // Success.
                result = r * sign;
                return fracDigits;
            }
            catch(Exception) {
                result = 0;
                return -1;
            }
        }
    // Test
        [Conditional("DEBUG")]
        public static void Tests()
        {
            Tests_double();
            Tests_float();
        }
        public static void Tests_double()
        {
            double n;
            bool f;

            f = TryParseDecimal("0", 0, -1, out n); Debug.Assert(f == true  && n == 0d);
            f = TryParseDecimal("1", 0, -1, out n); Debug.Assert(f == true  && n == 1d);
            f = TryParseDecimal("9", 0, -1, out n); Debug.Assert(f == true  && n == 9d);
            f = TryParseDecimal("a", 0, -1, out n); Debug.Assert(f == false && n == 0d);

            f = TryParseDecimal("zyz0", 0, -1, out n); Debug.Assert(f == true  && n == 0d);
            f = TryParseDecimal("zyz1", 0, -1, out n); Debug.Assert(f == true  && n == 1d);
            f = TryParseDecimal("zyz9", 0, -1, out n); Debug.Assert(f == true  && n == 9d);
            f = TryParseDecimal("zyza", 0, -1, out n); Debug.Assert(f == false && n == 0d);

            f = TryParseDecimal("0xyz", 0, -1, out n); Debug.Assert(f == true  && n == 0d);
            f = TryParseDecimal("1xyz", 0, -1, out n); Debug.Assert(f == true  && n == 1d);
            f = TryParseDecimal("9xyz", 0, -1, out n); Debug.Assert(f == true  && n == 9d);
            f = TryParseDecimal("axyz", 0, -1, out n); Debug.Assert(f == false && n == 0d);

            f = TryParseDecimal("0"   , 0, 1, out n); Debug.Assert(f == true  && n == 0d);
            f = TryParseDecimal("xxx0", 0, 4, out n); Debug.Assert(f == true  && n == 0d);
            f = TryParseDecimal("0xxx", 0, 4, out n); Debug.Assert(f == true  && n == 0d);

            f = TryParseDecimal("0"   , 0, 0, out n); Debug.Assert(f == false && n == 0d);
            f = TryParseDecimal("0"   , 1, 1, out n); Debug.Assert(f == false && n == 0d);
            f = TryParseDecimal("0"   , 1, 0, out n); Debug.Assert(f == false && n == 0d);

            f = TryParseDecimal("0.1"    , 0, 1, out n); Debug.Assert(f == true  && n == 0d);
            f = TryParseDecimal("0.12"   , 0, 1, out n); Debug.Assert(f == true  && n == 0d);
            f = TryParseDecimal("0.1234" , 0, 1, out n); Debug.Assert(f == true  && n == 0d);

            f = TryParseDecimal("0.1"    , 0, 2, out n); Debug.Assert(f == true  && n == 0d);
            f = TryParseDecimal("0.12"   , 0, 2, out n); Debug.Assert(f == true  && n == 0d);
            f = TryParseDecimal("0.1234" , 0, 2, out n); Debug.Assert(f == true  && n == 0d);

            f = TryParseDecimal("0.1"    , 0, 3, out n); Debug.Assert(f == true  && n == 0.1d);
            f = TryParseDecimal("0.12"   , 0, 3, out n); Debug.Assert(f == true  && n == 0.1d);
            f = TryParseDecimal("0.1234" , 0, 3, out n); Debug.Assert(f == true  && n == 0.1d);

            f = TryParseDecimal("0.1"    , 0, -1, out n); Debug.Assert(f == true  && n == 0.1d);
            f = TryParseDecimal("0.12"   , 0, -1, out n); Debug.Assert(f == true  && n == 0.12d);
            f = TryParseDecimal("0.1234" , 0, -1, out n); Debug.Assert(f == true  && n == 0.1234d);

            f = TryParseDecimal(".1"    , 0, -1, out n); Debug.Assert(f == true  && n == 0.1d);
            f = TryParseDecimal(".12"   , 0, -1, out n); Debug.Assert(f == true  && n == 0.12d);
            f = TryParseDecimal(".1234" , 0, -1, out n); Debug.Assert(f == true  && n == 0.1234d);

            f = TryParseDecimal("1."    , 0, -1, out n); Debug.Assert(f == true  && n == 1d);
            f = TryParseDecimal("1.2"   , 0, -1, out n); Debug.Assert(f == true  && n == 1.2d);
            f = TryParseDecimal("1.234" , 0, -1, out n); Debug.Assert(f == true  && n == 1.234d);

            f = TryParseDecimal("12."   , 0, -1, out n); Debug.Assert(f == true  && n == 12d);
            f = TryParseDecimal("12.34" , 0, -1, out n); Debug.Assert(f == true  && n == 12.34d);

            f = TryParseDecimal("eee..f.g-h12.34" , 0, -1, out n); Debug.Assert(f == true  && n == 12.34d);
            f = TryParseDecimal("eee..f.g-h.34"    , 0, -1, out n); Debug.Assert(f == true  && n == 0.34d);

            f = TryParseDecimal("-12."   , 0, -1, out n); Debug.Assert(f == true  && n == -12d);
            f = TryParseDecimal("-12.34" , 0, -1, out n); Debug.Assert(f == true  && n == -12.34d);

            f = TryParseDecimal("-12.-"   , 0, -1, out n); Debug.Assert(f == true  && n == -12d);
            f = TryParseDecimal("-12.34-" , 0, -1, out n); Debug.Assert(f == true  && n == -12.34d);

            f = TryParseDecimal("-1-2.-"   , 0, -1, out n); Debug.Assert(f == true  && n == -1d);
            f = TryParseDecimal("-12-.34-" , 0, -1, out n); Debug.Assert(f == true  && n == -12d);
            f = TryParseDecimal("-12.-34-" , 0, -1, out n); Debug.Assert(f == true  && n == -12d);
            f = TryParseDecimal("-12.3-4-" , 0, -1, out n); Debug.Assert(f == true  && n == -12.3d);

            f = TryParseDecimal("1-2.-"   , 0, -1, out n); Debug.Assert(f == true  && n == 1d);
            f = TryParseDecimal("12-.34-" , 0, -1, out n); Debug.Assert(f == true  && n == 12d);
            f = TryParseDecimal("12.-34-" , 0, -1, out n); Debug.Assert(f == true  && n == 12d);
            f = TryParseDecimal("12.3-4-" , 0, -1, out n); Debug.Assert(f == true  && n == 12.3d);

        }
        public static void Tests_float()
        {
            float n;
            bool f;

            f = TryParseDecimal("0", 0, -1, out n); Debug.Assert(f == true  && n == 0f);
            f = TryParseDecimal("1", 0, -1, out n); Debug.Assert(f == true  && n == 1f);
            f = TryParseDecimal("9", 0, -1, out n); Debug.Assert(f == true  && n == 9f);
            f = TryParseDecimal("a", 0, -1, out n); Debug.Assert(f == false && n == 0f);

            f = TryParseDecimal("zyz0", 0, -1, out n); Debug.Assert(f == true  && n == 0f);
            f = TryParseDecimal("zyz1", 0, -1, out n); Debug.Assert(f == true  && n == 1f);
            f = TryParseDecimal("zyz9", 0, -1, out n); Debug.Assert(f == true  && n == 9f);
            f = TryParseDecimal("zyza", 0, -1, out n); Debug.Assert(f == false && n == 0f);

            f = TryParseDecimal("0xyz", 0, -1, out n); Debug.Assert(f == true  && n == 0f);
            f = TryParseDecimal("1xyz", 0, -1, out n); Debug.Assert(f == true  && n == 1f);
            f = TryParseDecimal("9xyz", 0, -1, out n); Debug.Assert(f == true  && n == 9f);
            f = TryParseDecimal("axyz", 0, -1, out n); Debug.Assert(f == false && n == 0f);

            f = TryParseDecimal("0"   , 0, 1, out n); Debug.Assert(f == true  && n == 0f);
            f = TryParseDecimal("xxx0", 0, 4, out n); Debug.Assert(f == true  && n == 0f);
            f = TryParseDecimal("0xxx", 0, 4, out n); Debug.Assert(f == true  && n == 0f);

            f = TryParseDecimal("0"   , 0, 0, out n); Debug.Assert(f == false && n == 0f);
            f = TryParseDecimal("0"   , 1, 1, out n); Debug.Assert(f == false && n == 0f);
            f = TryParseDecimal("0"   , 1, 0, out n); Debug.Assert(f == false && n == 0f);

            f = TryParseDecimal("0.1"    , 0, 1, out n); Debug.Assert(f == true  && n == 0f);
            f = TryParseDecimal("0.12"   , 0, 1, out n); Debug.Assert(f == true  && n == 0f);
            f = TryParseDecimal("0.1234" , 0, 1, out n); Debug.Assert(f == true  && n == 0f);

            f = TryParseDecimal("0.1"    , 0, 2, out n); Debug.Assert(f == true  && n == 0f);
            f = TryParseDecimal("0.12"   , 0, 2, out n); Debug.Assert(f == true  && n == 0f);
            f = TryParseDecimal("0.1234" , 0, 2, out n); Debug.Assert(f == true  && n == 0f);

            f = TryParseDecimal("0.1"    , 0, 3, out n); Debug.Assert(f == true  && n == 0.1f);
            f = TryParseDecimal("0.12"   , 0, 3, out n); Debug.Assert(f == true  && n == 0.1f);
            f = TryParseDecimal("0.1234" , 0, 3, out n); Debug.Assert(f == true  && n == 0.1f);

            f = TryParseDecimal("0.1"    , 0, -1, out n); Debug.Assert(f == true  && n == 0.1f);
            f = TryParseDecimal("0.12"   , 0, -1, out n); Debug.Assert(f == true  && n == 0.12f);
            f = TryParseDecimal("0.1234" , 0, -1, out n); Debug.Assert(f == true  && n == 0.1234f);

            f = TryParseDecimal(".1"    , 0, -1, out n); Debug.Assert(f == true  && n == 0.1f);
            f = TryParseDecimal(".12"   , 0, -1, out n); Debug.Assert(f == true  && n == 0.12f);
            f = TryParseDecimal(".1234" , 0, -1, out n); Debug.Assert(f == true  && n == 0.1234f);

            f = TryParseDecimal("1."    , 0, -1, out n); Debug.Assert(f == true  && n == 1f);
            f = TryParseDecimal("1.2"   , 0, -1, out n); Debug.Assert(f == true  && n == 1.2f);
            f = TryParseDecimal("1.234" , 0, -1, out n); Debug.Assert(f == true  && n == 1.234f);

            f = TryParseDecimal("12."   , 0, -1, out n); Debug.Assert(f == true  && n == 12f);
            f = TryParseDecimal("12.34" , 0, -1, out n); Debug.Assert(f == true  && n == 12.34f);

            f = TryParseDecimal("eee..f.g-h12.34" , 0, -1, out n); Debug.Assert(f == true  && n == 12.34f);
            f = TryParseDecimal("eee..f.g-h.34"    , 0, -1, out n); Debug.Assert(f == true  && n == 0.34f);

            f = TryParseDecimal("-12."   , 0, -1, out n); Debug.Assert(f == true  && n == -12f);
            f = TryParseDecimal("-12.34" , 0, -1, out n); Debug.Assert(f == true  && n == -12.34f);

            f = TryParseDecimal("-12.-"   , 0, -1, out n); Debug.Assert(f == true  && n == -12f);
            f = TryParseDecimal("-12.34-" , 0, -1, out n); Debug.Assert(f == true  && n == -12.34f);

            f = TryParseDecimal("-1-2.-"   , 0, -1, out n); Debug.Assert(f == true  && n == -1f);
            f = TryParseDecimal("-12-.34-" , 0, -1, out n); Debug.Assert(f == true  && n == -12f);
            f = TryParseDecimal("-12.-34-" , 0, -1, out n); Debug.Assert(f == true  && n == -12f);
            f = TryParseDecimal("-12.3-4-" , 0, -1, out n); Debug.Assert(f == true  && n == -12.3f);

            f = TryParseDecimal("1-2.-"   , 0, -1, out n); Debug.Assert(f == true  && n == 1f);
            f = TryParseDecimal("12-.34-" , 0, -1, out n); Debug.Assert(f == true  && n == 12f);
            f = TryParseDecimal("12.-34-" , 0, -1, out n); Debug.Assert(f == true  && n == 12f);
            f = TryParseDecimal("12.3-4-" , 0, -1, out n); Debug.Assert(f == true  && n == 12.3f);
        }
    }
}
