using System.Web;

namespace i18n
{
    /// <summary>
    /// 
    /// </summary>
    public class HtmlString : IHtmlString
    {
        private readonly IHtmlStringFormatter _formatter;
        private readonly string _text;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public HtmlString(string text) : this()
        {
            _text = text;
        }

        /// <summary>
        /// 
        /// </summary>
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