using System;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace i18n
{
    /// <summary>
    /// A global filter for automatically redirecting localized requests
    /// to appropriate URLs, as well as setting the resulting language in
    /// the HTTP response
    /// </summary>
    public class LanguageFilter : IActionFilter, IResultFilter
    {
        private const string ContentLanguageHeader = "Content-Language";
        private readonly I18NSession _session;
        private readonly ILocalizingService _service;

        public LanguageFilter()
        {
            _session = new I18NSession();
            _service = DefaultSettings.LocalizingService;
        }

        /// <summary>
        /// Called before an action method executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            switch (DefaultSettings.TheMode)
            {
                case DefaultSettings.Mode.Basic:
                {
                    var request = filterContext.HttpContext.Request;
                    var values = filterContext.RouteData.Values;
                    string str;

                    // If current route is NOT to be localized....nothing to do.
                    if (filterContext.RouteData.Route.NoLocalize()) {
                        break; }

                    // Value is already injected from a route declaration
                    if (values.ContainsKey("language"))
                    {
                        return;
                    }

                    // Value is part of the explicit route and is the preferred language (from those available)
                    var preferred = _session.GetLanguageFromSessionOrService(filterContext.HttpContext);
                    var url = _session.GetUrlFromRequest(filterContext.HttpContext.Request);
                    if (url.EndsWithAnyIgnoreCase(string.Format("/{0}", preferred), string.Format("/{0}/", preferred)) != null)
                    {
                        return;
                    }

                    // Value is loose in the query string, i.e. '/?language=en'
                    if ((str = request.QueryString["language"]) != null)
                    {
                        _session.Set(filterContext.HttpContext, str);
                        preferred = _session.GetLanguageFromSessionOrService(filterContext.HttpContext);
                    }

                    // Value is part of the explicit route, i.e. '/about/en' but not the preferred language
                    var languages = request.UserLanguages ?? new[] { I18N.DefaultTwoLetterISOLanguageName };
                    foreach (var language in languages.Where(language => !string.IsNullOrWhiteSpace(language)))
                    {
                        var semiColonIndex = language.IndexOf(';');
                        var token = string.Format("/{0}", semiColonIndex > -1 ? language.Substring(0, semiColonIndex) : language);
                        if (url.EndsWithAnyIgnoreCase(token, string.Format("{0}/", token)) == null)
                        {
                            continue;
                        }

                        // This is an explicit language request, override preferences
                        _session.Set(filterContext.HttpContext, token.Substring(1));
                        preferred = _session.GetLanguageFromSessionOrService(filterContext.HttpContext);
                        break;
                    }
            
                    RedirectWithLanguage(filterContext, values, preferred);
                    break;
                }
                case DefaultSettings.Mode.Enhanced:
                {
                // We get here with conceivably with route datatokens relating to language tags found in 
                // the URL set by LanguageRouteDecorator.
                //
                // We now run the Request Language Selection algorithm to determine the PrincipalAppLanguage
                // (PAL) for the rest of the request. This may involve issuing a redirect result to get the
                // user agent on a URL that is an AppLanguage.
                //
                // 1. If langtag is in URL but does not equal an AppLanguage...redirect to the 
                //    closest matching AppLanguage (falling back to default language if necessary).
                //    We can then expect to get back here with a the new request and effectively goto 2..
                //
                // 2. If langtag is in URL and does equal an AppLanguage...establish the PAL for the request
                //    and done.
                //
                // If we get here, we can asumme that the url doesn't contain a langtag.
                //
                // 3. If langtag cookie exists, if it matches an AppLanguage (directly
                //    or indirectly through language-matching), add it to the URL and
                //    redirect to that new URL.
                //
                // 4. Failing all the above, determine PAL from any UserLanguages (Accept-Language header),
                //    prepend it to the URL and redirect.
                //
                // NB: if it were possible to issue a redirect from LanguageRouteDecorator.GetRouteData
                // then this code may be refactored to there. It probably is with some work:
                // http://haacked.com/archive/2011/02/02/redirecting-routes-to-maintain-persistent-urls.aspx
                //
                    // Only interested in normal, external actions.
                    if (filterContext.IsChildAction) {
                        break; }

                    // If current route is NOT to be localized....nothing to do.
                    if (filterContext.RouteData.Route.NoLocalize()) {
                        break; }

                    // Init.
                    LanguageTag urlLangTag = (LanguageTag)filterContext.RouteData.DataTokens["i18n.langtag_url"];
                    LanguageTag appLangTag = (LanguageTag)filterContext.RouteData.DataTokens["i18n.langtag_app"];

                    // 1.
                    if (urlLangTag.IsValid()
                        && appLangTag.IsValid()
                        && !urlLangTag.Equals(appLangTag))
                    {
                       // Construct new URL.
                        string urlOrg = filterContext.HttpContext.Request.Url.ToString();
                        string langTagPrefix = string.Format("/{0}", urlLangTag);
                        int pos = urlOrg.IndexOf(langTagPrefix);
                        if (pos == -1) {
                            throw new System.ApplicationException(); }
                        string urlNew = string.Format("{0}{1}{2}{3}", 
                            urlOrg.Substring(0, pos), 
                            "/", 
                            appLangTag,
                            urlOrg.Substring(pos + langTagPrefix.Length));
                       // Redirect user agent to new URL.
                        var result = new RedirectResult(urlNew, DefaultSettings.PermanentRedirects);
                        result.ExecuteResult(filterContext);
                        break;
                    }

                    // 2.
                    if (urlLangTag.IsValid()
                        && appLangTag.IsValid()
                        && urlLangTag.Equals(appLangTag))
                    {
                        filterContext.HttpContext.SetPrincipalAppLanguageForRequest(appLangTag);
                        break;
                    }

                    // 3.
                    HttpCookie cookie_langtag = filterContext.HttpContext.Request.Cookies.Get("i18n.langtag");
                    if (cookie_langtag != null) {
                        appLangTag = LanguageHelpers.GetMatchingAppLanguage(cookie_langtag.Value); }
                    // 4.
                    if (appLangTag == null) {
                        appLangTag = LanguageHelpers.GetMatchingAppLanguage(filterContext.HttpContext.GetRequestUserLanguages()); }
                    // If we have got a PAL (from either the cookie or UserLanguages)...redirect to new URL based on it.
                    if (appLangTag != null)
                    {
                        Uri urlOrg = filterContext.HttpContext.Request.Url;
                        UriBuilder urlNew = new UriBuilder(urlOrg);
                        urlNew.PrependPath(appLangTag.ToString());
                        // Redirect user agent to new URL.
                        var result = new RedirectResult(urlNew.ToString(), DefaultSettings.PermanentRedirects);
                        result.ExecuteResult(filterContext);
                        break;
                    }

                    // No changes. We don't expect to get here as the final GetMatchingAppLanguage call
                    // above should fallback on the default AppLanguage.
                    break;
                }
                default:
                    throw new System.ApplicationException();
            }
        }
        
        private static void RedirectWithLanguage(ControllerContext filterContext, RouteValueDictionary values, string language)
        {
            if(!values.ContainsKey("language"))
            {
                values.Add("language", language);
            }

            var helper = new UrlHelper(filterContext.RequestContext);
            var url = helper.RouteUrl(values);

            var result = new RedirectResult(url, DefaultSettings.PermanentRedirects);
            result.ExecuteResult(filterContext);
        }

        /// <summary>
        /// Called after the action method executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            switch (DefaultSettings.TheMode)
            {
                case DefaultSettings.Mode.Basic:
                {
                    var language = I18NSession.GetLanguageFromSession(filterContext.HttpContext)
                                   ?? _service.GetBestAvailableLanguageFrom(filterContext.HttpContext.Request.UserLanguages)
                                   ?? I18N.DefaultTwoLetterISOLanguageName;

                    filterContext.HttpContext.Response.AppendHeader(ContentLanguageHeader, language);
                    break;
                }
                case DefaultSettings.Mode.Enhanced:
                {
                    break;
                }
                default:
                    throw new System.ApplicationException();
            }
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }
        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        // This method called AFTER any view is generated.
        //
            switch (DefaultSettings.TheMode)
            {
                case DefaultSettings.Mode.Basic:
                {
                    break;
                }
                case DefaultSettings.Mode.Enhanced:
                {
                   // Add a Content-Language HTTP header to the response.
                   // NB: this is dependent on per-language counts incremented by the
                   // GetText calls made during view generation etc..
                    filterContext.HttpContext.SetContentLanguageHeader();
                    break;
                }
                default:
                    throw new System.ApplicationException();
            }
        }
    }
}
