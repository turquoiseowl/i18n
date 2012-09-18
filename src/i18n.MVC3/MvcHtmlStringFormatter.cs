using System.Web;
using System.Web.Mvc;

namespace i18n
{
    public class MvcHtmlStringFormatter : IHtmlStringFormatter 
    {
        public IHtmlString Format(string input)
        {
            return new MvcHtmlString(input);
        }
    }
}
