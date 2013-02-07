using System.Web;

namespace i18n
{
    /// <summary>
    /// Defines an alias for passing localizable text
    /// </summary>
    public interface ILocalizing
    {
    // TODO: This interface appears to be redundant unless it is used externally by reflection somehow (Issue #36).
    // If it IS to be maintained, it should perhaps be augmented with the overloads and variations of the _()
    // method as implemented by the likes of LocalizingWebViewPage.
    //
        /// <summary>
        /// Looks up and returns any translation available of the given text.
        /// </summary>
        /// <param name="text">The text to localize.</param>
        /// <returns>
        /// Either a translation of the text or if none found, returns text as is.
        /// </returns>
        IHtmlString _(string text);
    }
}
