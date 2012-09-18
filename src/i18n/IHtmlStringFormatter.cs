using System.Web;

namespace i18n
{
    public interface IHtmlStringFormatter
    {
        IHtmlString Format(string input);
    }
}