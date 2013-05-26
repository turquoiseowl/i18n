using System.Web;
using System.Web.Mvc;

namespace i18n
{
    /// <summary>
    /// 
    /// </summary>
    public static class ModelStateDictionaryExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="errorMessage"></param>
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

        /// <summary>
        /// 
        /// </summary>
        protected I18NController()
        {
            _session = new I18NSession();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public virtual IHtmlString _(string text)
        {
            return new MvcHtmlString(_session.GetText(HttpContext, text));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public virtual string __(string text)
        {
            return _session.GetText(HttpContext, text);
        }
    }
}