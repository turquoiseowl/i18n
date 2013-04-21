using System.Web;

namespace i18n.DataAnnotations
{
    public class StringLengthAttribute : System.ComponentModel.DataAnnotations.StringLengthAttribute, ILocalizing, System.Web.Mvc.IClientValidatable
    {
        private readonly I18NSession _session;

        public StringLengthAttribute(int maximumLength): base(maximumLength)
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

        #region IClientValidatable Members

        public System.Collections.Generic.IEnumerable<System.Web.Mvc.ModelClientValidationRule> GetClientValidationRules(System.Web.Mvc.ModelMetadata metadata, System.Web.Mvc.ControllerContext context)
        {
            yield return new System.Web.Mvc.ModelClientValidationStringLengthRule
            (FormatErrorMessage(metadata.GetDisplayName()), this.MinimumLength, this.MaximumLength);
        }

        #endregion
    }
}