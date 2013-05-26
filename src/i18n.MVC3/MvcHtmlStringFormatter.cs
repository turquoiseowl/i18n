using System.Web;
using System.Web.Mvc;

namespace i18n
{
    /// <summary>
    /// 
    /// </summary>
    public class MvcHtmlStringFormatter : IHtmlStringFormatter 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IHtmlString Format(string input)
        {
            return new MvcHtmlString(input);
        }
    }
}
