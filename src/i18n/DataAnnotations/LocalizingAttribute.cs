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

        public virtual string _(string text)
        {
            return _session.GetText(HttpContext.Current, text);
        }
    }
}