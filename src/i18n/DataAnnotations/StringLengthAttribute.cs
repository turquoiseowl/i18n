using System.Web;

namespace i18n.DataAnnotations
{
    public class StringLengthAttribute : System.ComponentModel.DataAnnotations.StringLengthAttribute, ILocalizing
    {
        public StringLengthAttribute(int maximumLength): base(maximumLength)
        {
        }

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