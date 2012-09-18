using System;
using System.Web;

namespace i18n.DataAnnotations
{
    public class LocalizingAttribute : Attribute, ILocalizing
    {
        private readonly I18NSession _session;

        protected LocalizingAttribute()
        {
            _session = new I18NSession();   
        }

        public virtual IHtmlString _(string text)
        {
            return new HtmlString(_session.GetText(HttpContext.Current, text));
        }
    }
}