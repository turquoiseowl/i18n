namespace i18n.NamedStringFormater
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// From: http://haacked.com/archive/2009/01/04/fun-with-named-formats-string-parsing-and-edge-cases.aspx
    /// </summary>
    public static class NamedStringFormatter
    {
        public static string Format(this string format, object source)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            var formattedStrings = (from expression in SplitFormat(format)
                                    select expression.Eval(source)).ToArray();
            return String.Join("", formattedStrings);
        }

        private static IEnumerable<ITextExpression> SplitFormat(string format)
        {
            int exprEndIndex = -1;
            int expStartIndex;

            do
            {
                expStartIndex = format.IndexOfExpressionStart(exprEndIndex + 1);
                if (expStartIndex < 0)
                {
                    //everything after last end brace index.
                    if (exprEndIndex + 1 < format.Length)
                    {
                        yield return new LiteralFormat(
                            format.Substring(exprEndIndex + 1));
                    }
                    break;
                }

                if (expStartIndex - exprEndIndex - 1 > 0)
                {
                    //everything up to next start brace index
                    yield return new LiteralFormat(format.Substring(exprEndIndex + 1
                      , expStartIndex - exprEndIndex - 1));
                }

                int endBraceIndex = format.IndexOfExpressionEnd(expStartIndex + 1);
                if (endBraceIndex < 0)
                {
                    //rest of string, no end brace (could be invalid expression)
                    yield return new FormatExpression(format.Substring(expStartIndex));
                }
                else
                {
                    exprEndIndex = endBraceIndex;
                    //everything from start to end brace.
                    yield return new FormatExpression(format.Substring(expStartIndex
                      , endBraceIndex - expStartIndex + 1));

                }
            } while (expStartIndex > -1);
        }

        static int IndexOfExpressionStart(this string format, int startIndex)
        {
            int index = format.IndexOf('{', startIndex);
            if (index == -1)
            {
                return index;
            }

            //peek ahead.
            if (index + 1 < format.Length)
            {
                char nextChar = format[index + 1];
                if (nextChar == '{')
                {
                    return IndexOfExpressionStart(format, index + 2);
                }
            }

            return index;
        }

        static int IndexOfExpressionEnd(this string format, int startIndex)
        {
            int endBraceIndex = format.IndexOf('}', startIndex);
            if (endBraceIndex == -1)
            {
                return endBraceIndex;
            }
            //start peeking ahead until there are no more braces...
            // }}}}
            int braceCount = 0;
            for (int i = endBraceIndex + 1; i < format.Length; i++)
            {
                if (format[i] == '}')
                {
                    braceCount++;
                }
                else
                {
                    break;
                }
            }
            if (braceCount % 2 == 1)
            {
                return IndexOfExpressionEnd(format, endBraceIndex + braceCount + 1);
            }

            return endBraceIndex;
        }
    }
}
