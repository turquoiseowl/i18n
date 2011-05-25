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
