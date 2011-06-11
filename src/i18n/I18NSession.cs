using System.Web;

namespace i18n
{
    /// <summary>
    /// A convenience class for localization operations
    /// </summary>
    public class I18NSession
    {
        private readonly ILocalizingService _service;
        private const string SessionKey = "po:language";

        public I18NSession()
        {
            _service = DependencyResolver.LocalizingService;
        }

        public virtual void Set(HttpContextBase context, string language)
        {
            if(context.Session != null)
            {
                context.Session[SessionKey] = language;
            }
        }

        public static string GetLanguageFromSession(HttpContext context)
        {
            return context.Session != null && context.Session[SessionKey] != null
                       ? context.Session[SessionKey].ToString()
                       : null;
        }

        public static string GetLanguageFromSession(HttpContextBase context)
        {
            return context.Session != null && context.Session[SessionKey] != null
                       ? context.Session[SessionKey].ToString()
                       : null;
        }

        public virtual string GetLanguageFromSessionOrService(HttpContextBase context)
        {
            var language = GetLanguageFromSession(context);
            if(language == null)
            {
                var languages = context.Request.UserLanguages;
                language = _service.GetBestAvailableLanguageFrom(languages);
                if (context.Session != null)
                {
                    context.Session.Add(SessionKey, language);
                }
            }
            return language;
        }

        public virtual string GetText(HttpContext context, string text)
        {
            // Prefer a stored value to browser-supplied preferences
            var stored = GetLanguageFromSession(context);
            if (stored != null)
            {
                return _service.GetText(text, new[] { stored });
            }

            // Use the client's browser settings to find a match
            var languages = context.Request.UserLanguages;
            return _service.GetText(text, languages);
        }

        public virtual string GetText(HttpContextBase context, string text)
        {
            // Prefer a stored value to browser-supplied preferences
            var stored = GetLanguageFromSession(context);
            if (stored != null)
            {
                text = _service.GetText(text, new[] { stored });
                return HttpUtility.HtmlDecode(text);
            }

            // Use the client's browser settings to find a match
            var languages = context.Request.UserLanguages;
            text = _service.GetText(text, languages);
            return HttpUtility.HtmlDecode(text);
        }

        public virtual string GetUrlFromRequest(HttpRequestBase context)
        {
            var url = context.RawUrl;
            if (url.EndsWith("/") && url.Length > 1)
            {
                // Support trailing slashes
                url = url.Substring(0, url.Length - 1);
            }
            return url;
        }
    }
}