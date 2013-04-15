using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace i18n.Helpers
{
    /// <summary>
    /// Describes a valid set of string tokens that define the format of a Nugget.
    /// </summary>
    /// <remarks>
    /// The standard numgget format is as follows:
    ///   [[[Enter between %0 and %1 characters|||{1}|||{2}/// The %0 identifies refers to min number and the %1 refers to the max number. ]]]
    /// where:
    ///   BeginToken = "[[["
    ///   EndToken = "]]]"
    ///   DelimiterToken = "|||"
    ///   CommentToken = "///"
    /// </remarks>
    public class NuggetTokens
    {
		public string BeginToken     { get; private set; }
		public string EndToken       { get; private set; }
		public string DelimiterToken { get; private set; }
        public string CommentToken   { get; private set; }

        public NuggetTokens(
		    string beginToken,
		    string endToken,
		    string delimiterToken,
		    string commentToken)
        {
            if (!beginToken.IsSet())     { throw new ArgumentNullException("beginToken"); }
            if (!endToken.IsSet())       { throw new ArgumentNullException("endToken"); }
            if (!delimiterToken.IsSet()) { throw new ArgumentNullException("delimiterToken"); }
            if (!commentToken.IsSet())   { throw new ArgumentNullException("commentToken"); }

            BeginToken = beginToken;
            EndToken = endToken;
            DelimiterToken = delimiterToken;
            CommentToken = commentToken;
        }
    }

    /// <summary>
    /// Describes the components of a nugget.
    /// </summary>
    /// <remarks>
    /// Formatted nuggets:
    ///
    /// The msgid for a formatted nugget:
    ///
    ///    Enter between %0 and %1 characters|||100|||6
    ///
    /// while the original string in the code for this may have been:
    ///
    ///    [[[Enter between %0 and %1 characters|||{1}|||{2}]]]
    ///
    /// The canonical msgid part is that between the opening [[[ and the first ||| or ///:
    ///
    ///    Enter between %0 and %1 characters
    /// </remarks>
    public class Nugget
    {
        public string MsgId { get; set; }
        public string[] FormatItems { get; set; }
        public string Comment { get; set; }

    // Helpers

        public bool IsFormatted
        {
            get {
                return FormatItems != null && FormatItems.Length != 0;
            }
        }

        public override string ToString()
        {
            return MsgId;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) {
                return false; }
            if (this.GetType() != obj.GetType()) {
                return false; }
            Nugget other = (Nugget)obj;
           // Compare non-array members.
            if (MsgId != other.MsgId // NB: the operator==() on string objects handles null value on either side just fine.
                || Comment != other.Comment) {
                return false; }
           // Compare arrays.
            if ((FormatItems == null) != (other.FormatItems == null)
                || (FormatItems != null && !FormatItems.SequenceEqual(other.FormatItems))) {
                return false; }
            return true;
        }

        public override int GetHashCode()
        {
            return 0
                .CombineHashCode(MsgId)
                .CombineHashCode(FormatItems)
                .CombineHashCode(Comment);
        }
    };

    /// <summary>
    /// Helper class for locating and processing nuggets in a string.
    /// </summary>
    public class NuggetParser
    {
        /// <summary>
        /// Set during CON to nugget definition tokens.
        /// </summary>
        NuggetTokens m_nuggetTokens;

        /// <summary>
        /// Initialized during CON to a regex suitable for breaking down a nugget into its component parts,
        /// as defined by the NuggetTokens definition passed to the CON.
        /// </summary>
        Regex m_regexNuggetBreakdown;

    // Con

        public NuggetParser(
		    NuggetTokens nuggetTokens)
        {
            m_nuggetTokens = nuggetTokens;
           // Prep the regexes. We escape each token char to ensure it is not misinterpreted.
           // · Breakdown e.g. "\[\[\[(.+?)(?:\|\|\|(.+?))*(?:\/\/\/(.+?))?\]\]\]"
            m_regexNuggetBreakdown = new Regex(
                string.Format(@"{0}(.+?)(?:{1}(.+?))*(?:{2}(.+?))?{3}",
                    EscapeString(m_nuggetTokens.BeginToken), 
                    EscapeString(m_nuggetTokens.DelimiterToken), 
                    EscapeString(m_nuggetTokens.CommentToken), 
                    EscapeString(m_nuggetTokens.EndToken)), 
                RegexOptions.CultureInvariant 
                    | RegexOptions.Singleline);
                        // RegexOptions.Singleline in fact enable multi-line nuggets.
        }

    // Operations

        /// <summary>
        /// Parses a string entity for nuggets, forwarding the nugget to a caller-provided
        /// delegate, with support for replacement of nugget strings in the entity.
        /// </summary>
        /// <param name="entity">
        /// String containing nuggets to be parsed. E.g. source code file, HTTP response entity.
        /// </param>
        /// <param name="ProcessNugget">
        /// Delegate callback to be called for each nugget encountered in entity:
        ///     delegate(string nuggetString, int pos, Nugget nugget1, string entity1).
        /// Returns a string with which to replace the nugget string in the source entity.
        /// If no change, then may return null.
        /// </param>
        /// <returns>
        /// Entity string reflecting any nugget strings replacements.
        /// </returns>
        public string ParseString(
            string entity, 
            Func<string, int, Nugget, string, string> ProcessNugget)
        {
        // Note that this method has two-levels of delegates:
        //   Outer delegate is the delegate which is called by regex as it matches each nugget
        //   Inner delegate is the client callback delegate (ProcessNugget) which we call from the outer delegate.
        //
           // Lookup any/all nuggets in the entity and call the client delegate (ProcessNugget) for each.
            return m_regexNuggetBreakdown.Replace(entity, delegate(Match match)
	        {
                Nugget nugget = InitNuggetFromRegexMatch(match);
               //
                string modifiedNuggetString = ProcessNugget(
                    match.Groups[0].Value, // entire nugget string
                    match.Groups[0].Index, // zero-based pos of the first char of entire nugget string
                    nugget,                // broken-down nugget
                    entity);               // source entity string
               // Returns either modified nugget string, or original nugget string (i.e. for no replacement).
                return modifiedNuggetString ?? match.Groups[0].Value;
	        });
        }

        /// <summary>
        /// Parses a nugget string to breakdown the nugget into individual components.
        /// </summary>
        /// <param name="nugget">Subject nugget string.</param>
        /// <returns>If successful, returns Nugget instance; otherwise returns null indicating a badly formatted nugget string.</returns>
        public Nugget BreakdownNugget(string nugget)
        {
            Match match = m_regexNuggetBreakdown.Match(nugget);
            return InitNuggetFromRegexMatch(match);
        }

    // Helpers

        /// <summary>
        /// Modifies a string such that each character is prefixed by another character
        /// (defaults to backslash).
        /// </summary>
        private static string EscapeString(string str, char escapeChar = '\\')
        {
            StringBuilder str1 = new StringBuilder(str.Length *2);
            foreach (var c in str) {
                str1.Append(escapeChar);
                str1.Append(c);
            }
            return str1.ToString();
        }

        /// <summary>
        /// Returns a nugget instance loaded from a regex match, or null if error.
        /// </summary>
        private Nugget InitNuggetFromRegexMatch(Match match)
        {
            if (!match.Success
                || match.Groups.Count != 4) {
                return null; }
            Nugget n = new Nugget();
           // Extract msgid from 2nd capture group.
            n.MsgId = match.Groups[1].Value;
           // Extract format items from 3rd capture group.
            var formatItems = match.Groups[2].Captures;
            if (formatItems.Count != 0) {
                n.FormatItems = new string[formatItems.Count];
                int i = 0;
                foreach (Capture capture in formatItems) {
                    if (!capture.Value.IsSet()) {
                        return null; } // bad format
                    n.FormatItems[i++] = capture.Value;
                }
            }
           // Extract comment from 4th capture group.
            if (match.Groups[3].Value.IsSet()) {
                n.Comment = match.Groups[3].Value; }
           // Success.
            return n;
        }
    }
}
