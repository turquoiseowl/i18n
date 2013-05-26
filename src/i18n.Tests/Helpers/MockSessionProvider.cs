using System.Collections.Specialized;
using System.Web;

namespace i18n.Tests.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class MockSessionProvider
    {
        public static readonly string DefaultLanguage = DefaultSettings.DefaultTwoLetterISOLanguageName;

        public I18NSession Session { get; set; }
        public LocalizingService LocalizingService { get; set; }
        public MockController Controller { get; private set; }

        protected HttpContextBase HttpContext
        {
            get
            {
                return Controller.ControllerContext.HttpContext;
            }
        }

        protected void Initialize()
        {
            Controller = MockHelper.FakeController();
            Controller.SetFakeControllerContext(new NameValueCollection
                                                    {
                                                        {"Accept-Language", "en"}
                                                    });
            Session = new I18NSession();
            LocalizingService = new LocalizingService();
        }

        protected string _(string text, HttpContext context)
        {
            return Session.GetText(context, text);
        }

        protected string _(string text, HttpContextBase context)
        {
            return Session.GetText(context, text);
        }

        protected string _(string text, LocalizingService service, string[] languages)
        {
            return LocalizingService.GetText(text, languages);
        }

    }
}