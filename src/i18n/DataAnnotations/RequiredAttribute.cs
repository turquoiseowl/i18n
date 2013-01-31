using System.Web;

namespace i18n.DataAnnotations
{
    public class RequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute, ILocalizing
    {
        public virtual IHtmlString _(string text)
        {
            return new HtmlString(HttpContext.Current.GetText(text));
        }

        public override string FormatErrorMessage(string name)
        {
            var formatted = base.FormatErrorMessage(name);
            return _(formatted).ToHtmlString();
        }
    }
}
