using System.Web;

namespace i18n
{
    public class HtmlString : IHtmlString
    {
        private readonly IHtmlStringFormatter _formatter;
        private readonly string _text;
        
        public HtmlString(string text) : this()
        {
            _text = text;
        }

        public HtmlString()
        {
            _formatter = DefaultSettings.HtmlStringFormatter;
        }

        public string ToHtmlString()
        {
            return _formatter.Format(_text).ToHtmlString();
        }
    }
}