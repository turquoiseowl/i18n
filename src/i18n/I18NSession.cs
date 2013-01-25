using System.Web;

namespace i18n
{
    /// <summary>
    /// A convenience class for localization operations
    /// </summary>
    public class I18NSession
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

        public static LanguageItem[] GetRequestUserLanguages(HttpContext context)
        {
            // Determine UserLanguages.
            // This value is created afresh first time this method is called per request,
            // and cached for the request's remaining calls to this method.
            LanguageItem[] UserLanguages = context.Items["i18n.UserLanguages"] as LanguageItem[];
            if (UserLanguages == null)
            {
                // Construct UserLanguages list and cache it for the rest of the request.
                context.Items["i18n.UserLanguages"] 
                    = UserLanguages 
                    = LanguageItem.ParseHttpLanguageHeader(context.Request.Headers["Accept-Language"]);
            }
            return UserLanguages;
        }
        public static LanguageItem[] GetRequestUserLanguages(HttpContextBase context)
        {
            // Determine UserLanguages.
            // This value is created afresh first time this method is called per request,
            // and cached for the request's remaining calls to this method.
            LanguageItem[] UserLanguages = context.Items["i18n.UserLanguages"] as LanguageItem[];
            if (UserLanguages == null)
            {
                // Construct UserLanguages list and cache it for the rest of the request.
                context.Items["i18n.UserLanguages"] 
                    = UserLanguages 
                    = LanguageItem.ParseHttpLanguageHeader(context.Request.Headers["Accept-Language"]);
            }
            return UserLanguages;
        }

        /// <summary>
        /// Helper for caching a per-request value that identifies the principal language
        /// under which the current request is to be handled.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <param name="pal">Selected AppLanguage.</param>
        public static void EstablishPrincipalAppLanguageForRequest(HttpContextBase context, LanguageTag pal)
        {
            context.Items["i18n.PAL"] = pal;
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
            // Prefer a stored value to browser-supplied preferences
            var stored = GetLanguageFromSession(context);
            string[] languages = stored != null ?
                languages = new[] { stored }:
                languages = context.Request.UserLanguages;

            switch (DefaultSettings.DefaultLanguageMatchingAlgorithm)
            {
                case DefaultSettings.LanguageMatching.Basic:
                {
                    text = DefaultSettings.LocalizingService.GetText(text, languages);
                    break;
                }
                case DefaultSettings.LanguageMatching.Enhanced:
                {
                    // Lookup resource.
                    LanguageTag lt;
                    text = DefaultSettings.LocalizingServiceEnhanced.GetText(text, GetRequestUserLanguages(context), out lt) ?? text;
                    break;
                }
                default:
                    throw new System.ApplicationException();
            }

            return HttpUtility.HtmlDecode(text);
        }
        public virtual string GetText(HttpContextBase context, string text)
        {
            // Prefer a stored value to browser-supplied preferences
            var stored = GetLanguageFromSession(context);
            string[] languages = stored != null ?
                languages = new[] { stored }:
                languages = context.Request.UserLanguages;

            switch (DefaultSettings.DefaultLanguageMatchingAlgorithm)
            {
                case DefaultSettings.LanguageMatching.Basic:
                {
                    text = DefaultSettings.LocalizingService.GetText(text, languages);
                    break;
                }
                case DefaultSettings.LanguageMatching.Enhanced:
                {
                    // Lookup resource.
                    LanguageTag lt;
                    text = DefaultSettings.LocalizingServiceEnhanced.GetText(text, GetRequestUserLanguages(context), out lt) ?? text;
                    break;
                }
                default:
                    throw new System.ApplicationException();
            }

            return HttpUtility.HtmlDecode(text);
        }
    }
}