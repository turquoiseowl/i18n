using System;
using System.Text.RegularExpressions;

namespace i18n.Domain.Entities
{
    public class ReferenceContext
    {
        public static bool ShowSourceContext { get; set; }

        public string Path { get; set; }
        public int LineNumber { get; set; }
        public string Context { get; set; }

        public static ReferenceContext Create(string path, string content, int position)
        {
            var lineNumber = 1;
            var lineStartPosition = 0;
            var context = string.Empty;

            for (var i = 0; i < content.Length; i++)
            {
                if (content[i] == '\n')
                {
                    lineNumber++;
                    lineStartPosition = i;
                }

                if (i < position) continue;

                var lineEndPosition = content.IndexOf("\n", i, StringComparison.Ordinal);

                if (lineEndPosition < 0)
                {
                    lineEndPosition = content.Length;
                }

                context = content.Substring(lineStartPosition, lineEndPosition - lineStartPosition);
                break;
            }

            return new ReferenceContext
            {
                Path = path,
                LineNumber = lineNumber,
                Context = context.Trim()
            };
        }

        private static readonly Regex ReferenceRegex = new Regex(@"\s*(?<path>.*):(?<lineNumber>\d+)(?<context>.*)$", RegexOptions.Compiled);

        public static ReferenceContext Parse(string line)
        {
            var match = ReferenceRegex.Match(line);

            if (match.Success)
            {
                return new ReferenceContext
                {
                    Path = match.Groups["path"].Value.Trim(),
                    LineNumber = Convert.ToInt32(match.Groups["lineNumber"].Value),
                    Context = match.Groups["context"].Value.Trim()
                };
            }

            return new ReferenceContext
            {
                Path = line
            };
        }

        public string ToComment()
        {
            var comment = Path + ":" + LineNumber;

            if (ShowSourceContext)
            {
                comment += " " + Context;
            }

            return comment;
        }
    }
}
