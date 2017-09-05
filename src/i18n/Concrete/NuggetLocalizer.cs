using System;
using System.Collections.Generic;
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
                _settings.NuggetCommentToken,
                _settings.NuggetParameterBeginToken,
                _settings.NuggetParameterEndToken
                ),
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
                message = _textLocalizer.GetText(
                    true, // true = try lookup with HtmlDecoded-msgid if lookup with raw msgid fails.
                    nugget.MsgId,
                    nugget.Comment,
                    languages,
                    out lt);
               //
                if (nugget.IsFormatted) {
                    var formatItems = new List<string>(nugget.FormatItems); // list of all parameters (both literals and nuggets)

                    // Extract attributes applied over existing variables - should be in this format: (((%ARGNO_AttributeName))) - e.g. (((%0_Gender))), (((%1_Number))), etc.
                    Dictionary<string, string> attributes = new Dictionary<string, string>();
                    message = m_regexAttributes.Replace(message, match =>
                    {
                        string attributeName = match.Groups["Attribute"].Value; // G (e.g. Gender, N for number, etc).
                        int parameterId = int.Parse(match.Groups["ParameterId"].Value); // 1
                        // if the attribute is (((%0_Gender) and parameter %0 is (((Customer))), paramterId is 0, and attributeKey will be the nugget translation for [[[Customer_Gender]]].
                        string parameterKey = nugget.FormatItems[parameterId];
                        if (parameterKey.StartsWith(_settings.NuggetParameterBeginToken) && parameterKey.EndsWith(_settings.NuggetParameterEndToken))
                            parameterKey= parameterKey.Substring(_settings.NuggetParameterBeginToken.Length, parameterKey.Length - _settings.NuggetParameterBeginToken.Length - _settings.NuggetParameterEndToken.Length);
                        var attributeKey = _settings.NuggetBeginToken + parameterKey + "_" + attributeName + _settings.NuggetEndToken;
                        if (!attributes.ContainsKey(attributeKey)) // if attribute wasn't yet extracted (calculated), then calculate its value, else get the cached value.
                        {
                            var attributeValue = ProcessNuggets(attributeKey, languages); 
                            formatItems.Add(attributeValue); // append value to the end of parameters list
                            attributes.Add(attributeKey, "%" + (formatItems.Count - 1)); // if we added element 3, refere to it as "%2"
                        }
                        return attributes[attributeKey];
                    });


                    // translate nuggets in parameters 
                    for (int i = 0; i < formatItems.Count; i++)
                    {
                        // if formatItem (parameter) is null or does not contain NuggetParameterBegintoken then continue
                        if (formatItems[i] == null || !formatItems[i].Contains(_settings.NuggetParameterBeginToken)) continue;

                        // replace parameter tokens with nugget tokens 
                        var fItem = formatItems[i];
                        if (fItem.StartsWith(_settings.NuggetParameterBeginToken) && fItem.EndsWith(_settings.NuggetParameterEndToken))
                            fItem = _settings.NuggetBeginToken
                            + fItem.Substring(_settings.NuggetParameterBeginToken.Length, fItem.Length - _settings.NuggetParameterBeginToken.Length - _settings.NuggetParameterEndToken.Length)
                            + _settings.NuggetEndToken;
                        // and process nugget 
                        formatItems[i] = ProcessNuggets(fItem, languages);
                    }

                    // Extracts and processes Conditionals. 
                    // uses same format as https://github.com/siefca/i18n-inflector:
                    // e.g. [[[Dear @0{f:Lady|m:Sir|n:You|All}!|||user.Gender]]]
                    // TODO: add support for combining variables. e.g. @num+person{s+1:I|*+2:You|s+3:%{person}|p+3:They|p+1:We} - http://www.rubydoc.info/gems/i18n-inflector/file/docs/EXAMPLES
                    message = m_regexConditionalIdentifiers.Replace(message, delegate (Match match)
                    {
                        int parameterId = int.Parse(match.Groups["ParameterId"].Value);
                        var parameterValue = formatItems[parameterId];
                        for (int i = 0; i < match.Groups["Value"].Captures.Count; i++)
                        {
                            string value = match.Groups["Value"].Captures[i].Value;
                            string Content = match.Groups["Content"].Captures[i].Value;
                            if (parameterValue.ToLower() == value.ToLower())
                                return Content;
                        }
                        return match.Groups["Default"].Value ?? "";
                    });


                    // Convert any identifies in a formatted nugget: %0 -> {0}
                    message = ConvertIdentifiersInMsgId(message);
                    // Format the message.
                    try {
                        message = string.Format(message, formatItems.ToArray()); }
                    catch (FormatException /*e*/) {
                        //message += string.Format(" [FORMAT EXCEPTION: {0}]", e.Message);
                        message += "[FORMAT EXCEPTION]";
                    }
                }
                // Optional late custom message translation modification.
                if (LocalizedApplication.Current.TweakMessageTranslation != null) {
                    message = LocalizedApplication.Current.TweakMessageTranslation(
                        System.Web.HttpContext.Current.GetHttpContextBase(),
                        nugget,
                        lt,
                        message); }
               // Output modified message (to be subsituted for original in the source entity).
                DebugHelpers.WriteLine("I18N.NuggetLocalizer.ProcessNuggets -- msgid: {0,35}, message: {1}", nugget.MsgId, message);
               //
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
                return message;
                    // NB: this was originally returning HttpUtility.HtmlEncode(message).
                    // Ref #105 and #202 as to why changed back to returning message as is.
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
        /// Regex for replacing conditional identifiers
        /// </summary>
        protected static Regex m_regexConditionalIdentifiers = new Regex(
            @"%(?<ParameterId>\d+){
            (?<Value>[^:|}])+:(?<Content>[^:|}]*)
            (\|(?<Value>[^:|}])+:(?<Content>[^:|}]*))*
              (\| (?<Default>[^:|}]*))?
            }", RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// Regex for replacing extension attributes over existing parameters
        /// </summary>
        protected static Regex m_regexAttributes = new Regex(
            @"\(\(\(%(?<ParameterId>\d+)_(?<Attribute>[^\(\)]*?)\)\)\)",
            RegexOptions.CultureInvariant);


        /// <summary>
        /// Sequence of chars used to delimit internal components of a Formatted nugget.
        /// </summary>
        public static string[] s_internalDelimiter = new string[] { "|||" };

    }
}
