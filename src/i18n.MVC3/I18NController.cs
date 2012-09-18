using System.Web;
using System.Web.Mvc;

namespace i18n
{
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
    public abstract class I18NController : Controller, ILocalizing
    {
        private readonly I18NSession _session;

        protected I18NController()
        {
            _session = new I18NSession();
        }

        public virtual IHtmlString _(string text)
        {
            return new MvcHtmlString(_session.GetText(HttpContext, text));
        }
    }
}