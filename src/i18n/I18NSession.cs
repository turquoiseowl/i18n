using System.Web;

namespace i18n
{
    /// <summary>
    /// A convenience class for localization operations
    /// </summary>
    public class I18NSession
        // TODO: This class requires some work and I suggest is possible redundant.
        // For instance, the Set and GetLanguageFromSession methods should be balanced
        // with respect to each other, yet one is virtual and the other is static.
        // It would make more sense for the GetLanguageFromSession methods to also be virtual
        // yet that would require a session param to be passed to GetText (when we are in Basic mode).
        // Enhanced mode does would not need this class at all if GetText was moved to be an extension
        // method of HttpContext (see HttpContextExtensions).
    {
        protected const string SessionKey = "po:language";

    // Static helpers

        public static string GetLanguageFromSession(HttpContext context)
        {
            object val;
            return context.Session != null && (val = context.Session[SessionKey]) != null
                       ? val.ToString()
                       : null;
        }
        public static string GetLanguageFromSession(HttpContextBase context)
        {
            object val;
            return context.Session != null && (val = context.Session[SessionKey]) != null
                       ? val.ToString()
                       : null;
        }

    // Overrideables

        public virtual void Set(HttpContextBase context, string language)
        {
            if(context.Session != null)
            {
                context.Session[SessionKey] = language;
            }
        }

        public virtual string GetLanguageFromSessionOrService(HttpContextBase context)
        {
            var language = GetLanguageFromSession(context);
            if(language == null)
            {
                var languages = context.Request.UserLanguages;
                language = DefaultSettings.LocalizingService.GetBestAvailableLanguageFrom(languages);
                if (context.Session != null)
                {
                    context.Session.Add(SessionKey, language);
                }
            }
            return language;
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

        public virtual string GetText(HttpContext context, string text)
        {
            switch (DefaultSettings.TheMode)
            {
                case DefaultSettings.Mode.Basic:
                {
                    // Prefer a stored value to browser-supplied preferences
                    var stored = GetLanguageFromSession(context);
                    string[] languages = stored != null ?
                        languages = new[] { stored }:
                        languages = context.Request.UserLanguages;
                    
                    text = DefaultSettings.LocalizingService.GetText(text, languages);
                    break;
                }
                case DefaultSettings.Mode.Enhanced:
                {
                    // Lookup resource.
                    LanguageTag lt;
                    text = DefaultSettings.LocalizingServiceEnhanced.GetText(text, context.GetRequestUserLanguages(), out lt) ?? text;
                    break;
                }
                default:
                    throw new System.ApplicationException();
            }

            return HttpUtility.HtmlDecode(text);
        }
        public virtual string GetText(HttpContextBase context, string text)
        {
            switch (DefaultSettings.TheMode)
            {
                case DefaultSettings.Mode.Basic:
                {
                    // Prefer a stored value to browser-supplied preferences
                    var stored = GetLanguageFromSession(context);
                    string[] languages = stored != null ?
                        languages = new[] { stored }:
                        languages = context.Request.UserLanguages;
                    
                    text = DefaultSettings.LocalizingService.GetText(text, languages);
                    break;
                }
                case DefaultSettings.Mode.Enhanced:
                {
                    // Lookup resource.
                    LanguageTag lt;
                    text = DefaultSettings.LocalizingServiceEnhanced.GetText(text, context.GetRequestUserLanguages(), out lt) ?? text;
                    break;
                }
                default:
                    throw new System.ApplicationException();
            }

            return HttpUtility.HtmlDecode(text);
        }
    }
}