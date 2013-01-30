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
        private readonly I18NSession _session;

        public LanguageRouteDecorator(RouteBase route) : base(route)
        {
            _session = new I18NSession();
        }

        public override RouteData GetRouteData(HttpContextBase context)
        {
            switch (DefaultSettings.TheMode)
            {
                case DefaultSettings.Mode.Basic:
                {
                    var result = _route.GetRouteData(context);

                    if(result == null)
                    {
                        var url = _session.GetUrlFromRequest(context.Request);
                        var languages = context.Request.UserLanguages ?? new[] { I18N.DefaultTwoLetterISOLanguageName };
                        foreach (var language in languages.Where(language => !string.IsNullOrWhiteSpace(language)))
                        {
                            var semiColonIndex = language.IndexOf(';');
                            var token = string.Format("/{0}", semiColonIndex > -1 ? language.Substring(0, semiColonIndex) : language);
                            string suffix = url.EndsWithAnyIgnoreCase(token, token + "/");
                            if (suffix == null)
                            {
                                continue;
                            }

                            // Truncate the language token from the url.
                            // NB: this can result in a zero-length URL, which if left as that
                            // will cause the ClonedHttpRequest.AppRelativeCurrentExecutionFilePath method
                            // to throw ArgumentNull exception when called indirectly. Thus, "" -> "/".
                            url = url.Substring(0, url.Length -suffix.Length);
                            if (url.Length == 0) {
                                url = "/"; }

                            var originalRequest = new ClonedHttpRequest(context.Request, url);
                            var originalContext = new ClonedHttpContext(context, originalRequest);
                                // TODO: possibly the above can be replaced by the following:
                                //      context.RewritePath(url);
                                // The rewrite back the original value afterwards.
                                // Or is there a reason not to do that?

                            result = _route.GetRouteData(originalContext);
                            if (result != null)
                            {
                                // Found the original non-decorated route
                                return result;
                            }
                        }
                    }

                    return result;
                }
                case DefaultSettings.Mode.Enhanced:
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
                    LanguageTag urlLangTag = LanguageTag.UrlExtractLangTag(urlOrg, out urlPatched);
                    if (urlLangTag.IsValid())
                    {
                        // If language matches an AppLanguage
                        LanguageTag appLangTag = LanguageHelpers.GetMatchingAppLanguage(urlLangTag.ToString());
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
                                routedata.DataTokens["i18n.langtag_url"] = urlLangTag;
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
                default:
                    throw new System.ApplicationException();
            }
        }

        public override VirtualPathData GetVirtualPath(RequestContext context, RouteValueDictionary values)
        {
            switch (DefaultSettings.TheMode)
            {
                case DefaultSettings.Mode.Basic:
                {
                    var result = _route.GetVirtualPath(context, values);

                    if (result != null && result.VirtualPath != null)
                    {
                        var request = context.HttpContext.Request;
                        if (!values.ContainsKey("language"))
                        {
                            if (request.QueryString["language"] != null)
                            {
                                result.VirtualPath = string.Format("{0}/{1}", result.VirtualPath, request.QueryString["language"]);
                            }
                            else
                            {
                                var language = _session.GetLanguageFromSessionOrService(context.HttpContext);
                                var token = string.Format("/{0}", language);
                                var url = _session.GetUrlFromRequest(context.HttpContext.Request);
                                if (url.EndsWithAnyIgnoreCase(token, string.Format("{0}/", token)) != null)
                                {
                                    result.VirtualPath = result.VirtualPath.Equals("")
                                                             ? language
                                                             : string.Format("{0}/{1}", result.VirtualPath, language);
                                }
                            }
                        }
                        else
                        {
                            // Use pre-injected route value
                            var baseUrl = context.HttpContext.Request.Url;
                            if (baseUrl != null)
                            {
                                var relativeUrl = new Uri(baseUrl, values["language"].ToString()).PathAndQuery.Substring(1);
                                    //BUGBUG: the above is erroneous as the Uri constructor does not simply append: it
                                    // assumes the first, baseUri argument has no local path component which is not
                                    // a safe assumption. E.g. /a/b + c = /a/c. I.e. the action get dropped.
                                    // A fix would probebly go something like this:
        /*
                                // Append language to relative url.
                                // E.g. account/signup?xyz -> account/signup/de?xyz
                                // NB: originally the Uri class was used here to combine the paths. This was a bug
                                // as the Uri constructor baseUri param assumes a Uri without a path, which is not
                                // a safe assumption here.
                                string relativeUrl = baseUrl.LocalPath;
                                if (!relativeUrl.EndsWith("/")) {
                                    relativeUrl += "/"; }
                                relativeUrl += values["language"] + baseUrl.Query;
                                relativeUrl = relativeUrl.Substring(1);
         */
                                    // However, reluctant to alter this code as it is not well documented and
                                    // don't want to break something else.

                                result.VirtualPath = relativeUrl;
                            }
                        }
                    }

                    return result;
                }
                case DefaultSettings.Mode.Enhanced:
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
                            result.VirtualPath = string.Format("{0}{1}{2}",
                                pal.ToString(),
                                result.VirtualPath.IsSet() ? "/" : "",
                                result.VirtualPath.IsSet() ? result.VirtualPath : "");
                        }
                    }
                    return result;
                }
                default:
                    throw new System.ApplicationException();
            }
        }
    }
}
