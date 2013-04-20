using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace i18n
{
    /// <summary>
    /// A route decorator that adapts existing original routes for localization
    /// </summary>
    internal class LanguageRouteDecorator : RouteDecoratorBase<LanguageRouteDecorator>
    {
        protected IUrlLocalizer m_localizer = LocalizedApplication.Current.UrlLocalizerService;

        public LanguageRouteDecorator(RouteBase route) : base(route)
        {
        }

    #region [RouteBase]

        public override RouteData GetRouteData(HttpContextBase context)
        {
        // MVC calls us here when it attempts to match the incoming request URL to
        // this particular registered route. Thus, we get here for each registered route until 
        // we report back a match with the route.
        // Our purpose is to check for any langtag embedded in the URL and remove it
        // for the purposes of doing a match:
        // 1. First examine URL for langtag prefix. If found and that matches an AppLanguage (directly
        //    or indirectly through language-matching), strip the langtag from the URL
        //    and try a match with the resulting URL. If found, add back the langtag
        //    and return the routedata for the match.
        // 2. Failing that, try a match with the URL as is.
        // 3. Failing that, return 'no match'. This will mean that if a language is in the URL but
        //    it is not supported, we 404.
        // 
            RouteData routedata;
            // 1.
            string urlOrg = context.Request.Url.AbsolutePath;
            string urlPatched;
            string urlLangTag = m_localizer.ExtractLangTagFromUrl(urlOrg, UriKind.Relative, true, out urlPatched);
            LanguageTag lt = LanguageTag.GetCachedInstance(urlLangTag);
            if (lt.IsValid())
            {
                // If language matches an AppLanguage
                LanguageTag appLangTag = LanguageHelpers.GetMatchingAppLanguage(urlLangTag);
                if (appLangTag != null)
                {
                    // Attempt to match the patched URL.
                    context.RewritePath(urlPatched);
                    routedata = _route.GetRouteData(context);
                    //urlOrg = string.Format("/{0}{1}", appLangTag.ToString(), urlPatched == "/" ? "" : urlPatched);
                    // Restore url to what it was before for subsequent tests.
                    context.RewritePath(urlOrg);
                    // If we have a match...store details of the language in the route, and success.
                    if (routedata != null) {
                        routedata.DataTokens["i18n.langtag_url"] = lt;
                            // langtag found in and stripped from the url.
                        routedata.DataTokens["i18n.langtag_app"] = appLangTag;
                            // Relative of langtag_url for which we know resource are available (AppLanguage).
                            // May be equal to langtag_url or a relative.
                        return routedata;
                    }
                }
            }
            // 2.
            routedata = _route.GetRouteData(context);
            if (routedata != null) {
                return routedata; }
            // 3.
            return null;
        }

        public override VirtualPathData GetVirtualPath(RequestContext context, RouteValueDictionary values)
        {
        // Here we do the reverse of what is done in GetRouteData.
        // That is, if this route matches the route values passed and a virtual path (URL path) is 
        // generated for the route, this will be a language neutral URL. Therefore, if the current
        // request has a Principal Application Language (PAL) set, it is approp. to insert that
        // langtag into the URL.
        //
            VirtualPathData result = _route.GetVirtualPath(context, values);
            // If route values match this route, and this route is NOT excluded from localization
            if (result != null 
                && result.VirtualPath != null
                && !this.NoLocalize())
            {
                // Get PAL was established for this request.
                // NB: if the PAL has not been set (for instance because the route was excluded
                // from localization) then we get the default language here.
                ILanguageTag pal = context.HttpContext.GetPrincipalAppLanguageForRequest();
                if (pal.IsValid()) {
                    // Prepend the virtual path with the PAL langtag.
                    // E.g. "account/signup" -> "fr-CH/account/signup"
                    // E.g. ""               -> "fr-CH"
                    result.VirtualPath = m_localizer.InsertLangTagIntoVirtualPath(
                        pal.ToString(), 
                        result.VirtualPath);
                }
            }
            return result;
        }

    #endregion
    }
}
