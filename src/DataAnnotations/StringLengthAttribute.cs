using System.Web;

namespace i18n.DataAnnotations
{
    public class StringLengthAttribute : System.ComponentModel.DataAnnotations.StringLengthAttribute, ILocalizing
    {
        private readonly I18NSession _session;

        public StringLengthAttribute(int maximumLength): base(maximumLength)
        {
            _session = new I18NSession();
        }

        public virtual string _(string text)
        {
            return _session.GetText(HttpContext.Current, text);
        }

        public override string FormatErrorMessage(string name)
        {
            var formatted = base.FormatErrorMessage(name);
            return _(formatted);
        }
    }
}