using System.Web;

namespace i18n
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHtmlStringFormatter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        IHtmlString Format(string input);
    }
}