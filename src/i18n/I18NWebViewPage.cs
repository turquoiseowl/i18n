using System.Web;
using System.Web.Mvc;

namespace i18n
{
    /// <summary>
    /// A base view providing an alias for localizable resources
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class I18NWebViewPage<T> : WebViewPage<T>, ILocalizing
    {
        private readonly I18NSession _session;

        protected I18NWebViewPage()
        {
            _session = new I18NSession();
        }
        
        public IHtmlString _(string text)
        {
            return new MvcHtmlString(_session.GetText(Context, text));
        }
    }

    /// <summary>
    /// A base view providing an alias for localizable resources
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class I18NWebViewPage : WebViewPage, ILocalizing
    {
        private readonly I18NSession _session;

        protected I18NWebViewPage()
        {
            _session = new I18NSession();
        }

        public IHtmlString _(string text)
        {
            return new MvcHtmlString(_session.GetText(Context, text));
        }
    }
}