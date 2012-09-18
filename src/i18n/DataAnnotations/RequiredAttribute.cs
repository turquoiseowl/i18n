using System.Web;

namespace i18n.DataAnnotations
{
    public class RequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute, ILocalizing
    {
        private readonly I18NSession _session;

        public RequiredAttribute()
        {
            _session = new I18NSession();   
        }

        public virtual IHtmlString _(string text)
        {
            return new HtmlString(_session.GetText(HttpContext.Current, text));
        }

        public override string FormatErrorMessage(string name)
        {
            var formatted = base.FormatErrorMessage(name);
            return _(formatted).ToHtmlString();
        }
    }
}
