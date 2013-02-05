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

        public string ProcessNuggets(string entity, ILocalizingService textLocalizer, LanguageItem[] languages)
        {
            // Lookup any/all msgid nuggets in the entity and replace with any translated message.
            return m_regexNugget.Replace(entity, delegate(Match match)
	        {
                if (textLocalizer == null) {
                    return "test.message"; }
	            string msgid = match.Groups[1].Value;

                // Lookup resource.
                LanguageTag lt;
                string message = textLocalizer.GetText(msgid, languages, out lt) ?? msgid;
                return HttpUtility.HtmlDecode(message);
	        });
        }

    #endregion

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
    }
}
