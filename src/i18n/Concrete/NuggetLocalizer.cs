using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using i18n.Helpers;
using i18n.Domain.Concrete;

namespace i18n
{
    /// <summary>
    /// The i18n default implementaion of the INuggetLocalizer service.
    /// </summary>
    public class NuggetLocalizer : INuggetLocalizer
    {
        private i18nSettings _settings;

        private ITextLocalizer _textLocalizer;

        private NuggetParser _nuggetParser;

        public NuggetLocalizer(
            i18nSettings settings,
            ITextLocalizer textLocalizer)
        {
            _settings = settings;
            _textLocalizer = textLocalizer;

            _nuggetParser = new NuggetParser(new NuggetTokens(
			    _settings.NuggetBeginToken,
			    _settings.NuggetEndToken,
			    _settings.NuggetDelimiterToken,
			    _settings.NuggetCommentToken),
                NuggetParser.Context.ResponseProcessing);
        }

    #region [INuggetLocalizer]

        public string ProcessNuggets(string entity, LanguageItem[] languages)
        {
           // Lookup any/all msgid nuggets in the entity and replace with any translated message.
            string entityOut = _nuggetParser.ParseString(entity, delegate(string nuggetString, int pos, Nugget nugget, string i_entity)
            {
            // Formatted nuggets:
            //
            // A formatted nugget will be encountered here like this, say:
            //
            //    [[[Enter between %0 and %1 characters|||100|||6]]]
            //
            // while the original string in the code for this may have been:
            //
            //    [[[Enter between %0 and %1 characters|||{1}|||{2}]]]
            //
            // The canonical msgid part is that between the opening [[[ and the first |||:
            //
            //    Enter between %0 and %1 characters
            //
            // We use that for the lookup.
            //
                LanguageTag lt;
                string message;
               // Check for unit-test caller.
                if (_textLocalizer == null) {
                    return "test.message"; }
               // Lookup resource using canonical msgid.
				message = _textLocalizer.GetText(HttpUtility.HtmlDecode(nugget.MsgId), HttpUtility.HtmlDecode(nugget.Comment), languages, out lt) ?? nugget.MsgId;
               //
                if (nugget.IsFormatted) {
                   // Convert any identifies in a formatted nugget: %0 -> {0}
                    message = ConvertIdentifiersInMsgId(message);
                   // Format the message.
                    var formatItems = new List<string>(nugget.FormatItems);

                    try {
                        // translate nuggets in parameters 
                        for (int i = 0; i < formatItems.Count; i++)
                        {
                            // if formatItem (parameter) is null or does not contain NuggetParameterBegintoken then continue
                            if (formatItems[i] == null || !formatItems[i].Contains(_settings.NuggetParameterBeginToken)) continue;

                            // replace parameter tokens with nugget tokens 
                            var fItem = formatItems[i].Replace(_settings.NuggetParameterBeginToken, _settings.NuggetBeginToken).Replace(_settings.NuggetParameterEndToken, _settings.NuggetEndToken);
                            // and process nugget 
                            formatItems[i] = ProcessNuggets(fItem, languages);
                        }

                        message = string.Format(message, formatItems.ToArray()); }
                    catch (FormatException /*e*/) {
                        //message += string.Format(" [FORMAT EXCEPTION: {0}]", e.Message);
                        message += "[FORMAT EXCEPTION]";
                    }
                }
               // Output modified message (to be subsituted for original in the source entity).
                DebugHelpers.WriteLine("I18N.NuggetLocalizer.ProcessNuggets -- msgid: {0,35}, message: {1}", nugget.MsgId, message);

                if (_settings.VisualizeMessages)
                {
                    string languageToken = string.Empty;
                    if (!string.IsNullOrWhiteSpace(_settings.VisualizeLanguageSeparator))
                        languageToken = lt.ToString() + _settings.VisualizeLanguageSeparator;
                    string endToken = _settings.NuggetVisualizeToken;
                    if (!string.IsNullOrWhiteSpace(_settings.NuggetVisualizeEndToken))
                        endToken = _settings.NuggetVisualizeEndToken;
                    message = string.Format("{0}{1}{2}{3}", _settings.NuggetVisualizeToken, languageToken, message, endToken);
                }
                return HttpUtility.HtmlEncode(message);
            });
           // Return modified entity.
            return entityOut;
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
        /// Helper for converting the C printf-style %0, %1 ... style identifiers in a formatted nugget msgid string
        /// to the .NET-style format items: {0}, {1} ...
        /// </summary>
        /// <remarks>
        /// A formatted msgid may be in the form:
        /// <para>
        /// Enter between %1 and %0 characters
        /// </para>
        /// <para>
        /// For which we return:
        /// </para>
        /// <para>
        /// Enter between {1} and {0} characters
        /// </para>
        /// </remarks>
        public static string ConvertIdentifiersInMsgId(string msgid)
        {
            // Convert %n style identifiers to {n} style.
            return m_regexPrintfIdentifiers.Replace(msgid, delegate(Match match)
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
        /// Regex for helping replace %0 style identifiers with {0} style ones.
        /// </summary>
        protected static Regex m_regexPrintfIdentifiers = new Regex(
            @"(%\d+)", 
            RegexOptions.CultureInvariant);

        /// <summary>
        /// Sequence of chars used to delimit internal components of a Formatted nugget.
        /// </summary>
        public static string[] s_internalDelimiter = new string[] { "|||" };

    }
}
