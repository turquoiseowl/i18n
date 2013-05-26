using System.Web;

namespace i18n
{
    /// <summary>
    /// Defines an alias for passing localizable text
    /// </summary>
    public interface ILocalizing
    {
        /// <summary>
        /// If available try returning a localized 
        /// string for given key.
        /// </summary>
        /// <param name="text">Text to localize</param>
        IHtmlString _(string text);
    }
}