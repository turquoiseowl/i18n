using System.Web;
using System.Web.Mvc;

namespace i18n
{
    using i18n.NamedStringFormater;

    public static class ModelStateDictionaryExtensions
    {
        public static void AddModelError(this ModelStateDictionary dictionary, string key, IHtmlString errorMessage)
        {
            dictionary.AddModelError(key, errorMessage.ToHtmlString());
        }
    }

    /// <summary>
    /// A base controller providing an alias for localizable resources
    /// </summary>
    public abstract class LocalizingController : Controller, ILocalizing
    {

    #region [ILocalizing]

        /// <summary>
        /// Looks up and returns any translation available of the given text.
        /// </summary>
        /// <param name="text">The text to localize.</param>
        /// <returns>
        /// Either a translation of the text or if none found, returns text as is.
        /// </returns>
        /// <remarks>
        /// This is one of the special alias methods recognised by the i18n library
        /// post-build process for extracting translatable strings from the project.
        /// </remarks>
        public virtual IHtmlString _(string text)
        {
            return new HtmlString(HttpContext.GetText(text));
        }
        public IHtmlString _(string text, params object[] parameters)
        {
            return new HtmlString(string.Format(HttpContext.GetText(text), parameters));
        }

        public IHtmlString _(string text, object source)
        {
            return new HtmlString(HttpContext.GetText(text).Format(source));
        }

    #endregion

        /// <summary>
        /// Looks up and returns a plain string containing any translation available of the given text.
        /// </summary>
        /// <param name="text">The text to localize.</param>
        /// <returns>
        /// Plain string containing either a translation of the text or if none found, the text as is.
        /// </returns>
        /// <remarks>
        /// This is one of the special alias methods recognised by the i18n library
        /// post-build process for extracting translatable strings from the project.
        /// </remarks>
        public virtual string __(string text)
        {
            return HttpContext.GetText(text);
        }
        public string __(string text, params object[] parameters)
        {
            return string.Format(HttpContext.GetText(text), parameters);
        }

        public string __(string text, object source)
        {
            return HttpContext.GetText(text).Format(source);
        }
    }
}
