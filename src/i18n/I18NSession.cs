using System.Web;

namespace i18n
{
    /// <summary>
    /// A convenience class for localization operations
    /// </summary>
    public class I18NSession
    {
        private const string SessionKey = "po:language";

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
                language = DependencyResolver.LocalizingService.GetBestAvailableLanguageFrom(languages);
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
                return DependencyResolver.LocalizingService.GetText(text, new[] { stored });
            }

            // Use the client's browser settings to find a match
            var languages = context.Request.UserLanguages;
            return DependencyResolver.LocalizingService.GetText(text, languages);
        }

        public virtual string GetText(HttpContextBase context, string text)
        {
            // Prefer a stored value to browser-supplied preferences
            var stored = GetLanguageFromSession(context);
            if (stored != null)
            {
                text = DependencyResolver.LocalizingService.GetText(text, new[] { stored });
                return HttpUtility.HtmlDecode(text);
            }

            // Use the client's browser settings to find a match
            var languages = context.Request.UserLanguages;
            text = DependencyResolver.LocalizingService.GetText(text, languages);
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