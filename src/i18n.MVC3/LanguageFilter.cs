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

    #region [IActionFilter]

        /// <summary>
        /// Called before an action method executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        // We get here with conceivably with route datatokens relating to language tags found in 
        // the URL set by LanguageRouteDecorator.
        //
        // We now run the Request Language Selection algorithm to determine the PrincipalAppLanguage
        // (PAL) for the rest of the request. This may involve issuing a redirect result to get the
        // user agent on a URL that is an AppLanguage.
        //
        // 1. If langtag is in URL but does not directly equal an AppLanguage (only indirectly)...redirect to the 
        //    closest matching AppLanguage (falling back to default language if necessary).
        //    We can then expect to get back here with the new request and effectively goto 2..
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
                return; }

            // If current route is NOT to be localized....nothing to do.
            if (filterContext.RouteData.Route.NoLocalize()) {
                return; }

            // Init.
            LanguageTag urlLangTag = (LanguageTag)filterContext.RouteData.DataTokens["i18n.langtag_url"];
            LanguageTag appLangTag = (LanguageTag)filterContext.RouteData.DataTokens["i18n.langtag_app"];

            // 1.
            if (urlLangTag.IsValid()
                && appLangTag.IsValid()
                && !urlLangTag.Equals(appLangTag))
            {
                RedirectWithLanguage(filterContext, appLangTag);
                return;
            }

            // 2.
            if (urlLangTag.IsValid()
                && appLangTag.IsValid()
                && urlLangTag.Equals(appLangTag))
            {
                filterContext.HttpContext.SetPrincipalAppLanguageForRequest(appLangTag);
                return;
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
                RedirectWithLanguage(filterContext, appLangTag);
                return;
            }

            // No changes. We don't expect to get here as the final GetMatchingAppLanguage call
            // above should fallback on the default AppLanguage.
        }
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

    #endregion

    #region [IResultFilter]

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }
        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        // This method called AFTER any view is generated.
        //
            // Add a Content-Language HTTP header to the response.
            // NB: this is dependent on per-language counts incremented by the
            // GetText calls made during view generation etc..
            filterContext.HttpContext.SetContentLanguageHeader();
        }

    #endregion

        protected static void RedirectWithLanguage(ControllerContext filterContext, LanguageTag langtag)
        {
            // Construct new URL.
            string urlNew = LocalizedApplication.UrlLocalizer.SetLangTagInUrl(
                filterContext.HttpContext.Request.Url.ToString(), 
                langtag.ToString());
            // Redirect user agent to new URL.
            var result = new RedirectResult(urlNew.ToString(), RouteLocalization.PermanentRedirects);
            result.ExecuteResult(filterContext);
        }
    }
}
