using System;
using System.Web;

namespace i18n.DataAnnotations
{
    public class LocalizingAttribute : Attribute, ILocalizing
    {
        public virtual IHtmlString _(string text)
        {
            return new HtmlString(HttpContext.Current.GetText(text));
        }
    }
}