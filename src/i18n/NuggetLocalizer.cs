using System;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;

namespace i18n
{
    /// <summary>
    /// The i18n default implementaion of the INuggetLocalizer service.
    /// </summary>
    public class NuggetLocalizer : INuggetLocalizer
    {

    #region INuggetLocalizer

        public string ProcessNuggets(string entity, ITextLocalizer textLocalizer, LanguageItem[] languages)
        {
            // Lookup any/all msgid nuggets in the entity and replace with any translated message.
            return m_regexNugget.Replace(entity, delegate(Match match)
	        {
                LanguageTag lt;
                string message;
                string tail;
                if (textLocalizer == null) {
                    return "test.message"; }
	            string msgid = match.Groups[1].Value;

                // Formatted nuggets:
                //
                // The msgid for a formatted nugget will be encountered here like this, say:
                //
                //    Enter between %0 and %1 characters|||100|||6
                //
                // while the original string in the code for this may have been:
                //
                //    [[[Enter between %0 and %1 characters|||{1}|||{2}]]]
                //
                // The canonical msgid part is that between the opening [[[ and the first |||:
                //
                //    Enter between %0 and %1 characters
                //
                // Thus we use that for the lookup.

                // If nugget is Formatted
                if (IsNuggetFormatted(msgid))
                {
                    // Extract canonical msgid part.
                    string msgid_canonical = GetCanonicalMsgIdFromNugget(msgid, out tail);

                    // Lookup resource using canonical msgid.
                    message = textLocalizer.GetText(msgid_canonical, languages, out lt);

                    // If not found...leave be.
                    if (message == null) {
                        return HttpUtility.HtmlDecode(msgid); }

                    // Extract values from tail of formatted nugget.
                    string[] tokens = tail.Split(s_internalDelimiter, StringSplitOptions.None);

                    // Format the message.
                    try {
                        message = string.Format(message, tokens); }
                    catch (FormatException e) {
                        //message += string.Format(" [FORMAT EXCEPTION: {0}]", e.Message);
                        message += "[FORMAT EXCEPTION]";
                    }
                }
                // For unformatted nugget
                else {
                    // Lookup resource.
                    message = textLocalizer.GetText(msgid, languages, out lt) ?? msgid;
                }

                DebugHelpers.WriteLine("I18N.NuggetLocalizer.ProcessNuggets -- msgid: {0,35}, message: {1}", msgid, message);
                return HttpUtility.HtmlDecode(message);
	        });
        }

    #endregion

    // Helpers

        /// <summary>
        /// Returns indication of whether the passed nugget is formatted or not.
        /// </summary>
        /// <param name="nugget">Subject nugget string.</param>
        /// <returns>true if formatted nugget, otherwise false.</returns>
        public static bool IsNuggetFormatted(string nugget)
        {
            return -1 != nugget.IndexOf(s_internalDelimiter[0]);
        }

        /// <summary>
        /// Helper for extracting and generating a canonical msgid from a formatted nugget.
        /// </summary>
        /// <param name="nugget"></param>
        /// <returns></returns>
        /// <remarks>
        /// A formatted msgid may be in the form:
        /// <para>
        /// Enter between %1 and %0 characters|||6|||100
        /// </para>
        /// <para>
        /// The canonical form of which would be:
        /// </para>
        /// <para>
        /// Enter between {1} and {0} characters
        /// </para>
        /// </remarks>
        public static string GetCanonicalMsgIdFromNugget(string nugget, out string tail)
        {
            // Truncate after any first ||| sequence.
            int pos = nugget.IndexOf(s_internalDelimiter[0]);
            if (-1 != pos) {
                tail = nugget.Substring(pos +s_internalDelimiter[0].Length);
                nugget = nugget.Substring(0, pos);
            }
            else {
                tail = string.Empty; }

            // Convert %n style identifiers to {n} style.
            return m_regexCanonicalNugget.Replace(nugget, delegate(Match match)
	        {
	            string s = match.Groups[1].Value;
                double id;
                if (ParseHelpers.TryParseDecimal(s, 1, s.Length -1 +1, out id)) {
                    s = string.Format("{{{0}}}", id); }
                return s;
	        });
        }

    // Implementation

        /// <summary>
        /// Regex for finding and replacing msgid nuggets.
        /// </summary>
        public static Regex m_regexNugget = new Regex(
            @"\[\[\[(.+?)\]\]\]", 
            RegexOptions.CultureInvariant);
            // [[[
            //      Match opening sequence.
            // .+?
            //      Lazily match chars up to
            // ]]]
            //      ... closing sequence

        /// <summary>
        /// Regex for helping replace %0 style identifiers with {0} style ones.
        /// </summary>
        protected static Regex m_regexCanonicalNugget = new Regex(
            @"(%\d+)", 
            RegexOptions.CultureInvariant);

        /// <summary>
        /// Sequence of chars used to delimit internal components of a Formatted nugget.
        /// </summary>
        public static string[] s_internalDelimiter = new string[] { "|||" };

    }
}
