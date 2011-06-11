using System.Web;

namespace i18n
{
    /// <summary>
    /// Defines an alias for passing localizable text
    /// </summary>
    public interface ILocalizing
    {
        /// <summary>
        /// Returns localized text for the given key, if available
        /// </summary>
        /// <param name="text">The text to localize</param>
        IHtmlString _(string text);
    }
}