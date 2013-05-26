using System;
using System.Web;

namespace i18n.DataAnnotations
{
    /// <summary>
    /// 
    /// </summary>
    public class LocalizingAttribute : Attribute, ILocalizing
    {
        private readonly I18NSession _session;

        protected LocalizingAttribute()
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
            return new HtmlString(_session.GetText(HttpContext.Current, text));
        }

    }
}